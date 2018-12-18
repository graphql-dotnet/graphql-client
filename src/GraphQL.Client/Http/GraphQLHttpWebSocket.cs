using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client.Http
{
	internal class GraphQLHttpWebSocket: IDisposable
	{
		private readonly Uri webSocketUri;
		private readonly GraphQLHttpClientOptions _options;
		private readonly byte[] buffer = new byte[1024 * 1024];
		private readonly ArraySegment<byte> arraySegment;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private Subject<GraphQLWebSocketResponse> _responseSubject = new Subject<GraphQLWebSocketResponse>();
		private Subject<GraphQLWebSocketRequest> _requestSubject = new Subject<GraphQLWebSocketRequest>();
		private IDisposable _requestSubscription;

		public WebSocketState WebSocketState => clientWebSocket?.State ?? WebSocketState.None;

		private WebSocket clientWebSocket = null;
		private int _connectionAttempt = 0;

		public GraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClientOptions options)
		{
			this.webSocketUri = webSocketUri;
			_options = options;
			arraySegment = new ArraySegment<byte>(buffer);
			ResponseStream = _createResponseStream();

			_requestSubscription = _requestSubject.Select(request => Observable.FromAsync(() => _sendWebSocketRequest(request))).Concat().Subscribe();
		}

		public IObservable<GraphQLWebSocketResponse> ResponseStream { get; }

		public Task SendWebSocketRequest(GraphQLWebSocketRequest request)
		{
			_requestSubject.OnNext(request);
			return request.SendTask();
		}

		private async Task _sendWebSocketRequest(GraphQLWebSocketRequest request)
		{
			try
			{
				if (_cancellationTokenSource.Token.IsCancellationRequested)
				{
					request.SendCanceled();
					return;
				}

				await InitializeWebSocket().ConfigureAwait(false);
				var webSocketRequestString = JsonConvert.SerializeObject(request);
				var arraySegmentWebSocketRequest = new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString));
				await this.clientWebSocket.SendAsync(arraySegmentWebSocketRequest, WebSocketMessageType.Text, true, _cancellationTokenSource.Token).ConfigureAwait(false);
				request.SendCompleted();
			}
			catch (Exception e)
			{
				request.SendFailed(e);
			}
		}

		public Task InitializeWebSocketTask { get; private set; } = Task.CompletedTask;

		#region Private Methods

		private Task _backOff()
		{
			_connectionAttempt++;

			if(_connectionAttempt == 1) return Task.CompletedTask;

			var delay = _options.BackOffStrategy(_connectionAttempt - 1);
			Debug.WriteLine($"connection attempt #{_connectionAttempt}, backing off for {delay.TotalSeconds} s");
			return Task.Delay(delay);
		}

		public Task InitializeWebSocket()
		{
			// do not attempt to initialize if cancellation is requested
			if(_disposed != null)
				throw new OperationCanceledException();

			// if an initialization task is already running, return that
			if(InitializeWebSocketTask != null &&
			   !InitializeWebSocketTask.IsFaulted &&
			   !InitializeWebSocketTask.IsCompleted)
				return InitializeWebSocketTask;

			// if the websocket is open, return a completed task
			if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
				return Task.CompletedTask;

			// else (re-)create websocket and connect
			clientWebSocket?.Dispose();

			// fix websocket not supported on win 7 using
			// https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
			clientWebSocket = SystemClientWebSocket.CreateClientWebSocket();
			switch (clientWebSocket)
			{
				case ClientWebSocket nativeWebSocket:
					nativeWebSocket.Options.AddSubProtocol("graphql-ws");
					break;
				case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
					managedWebSocket.Options.AddSubProtocol("graphql-ws");
					break;
				default:
					throw new NotSupportedException($"unknown websocket type {clientWebSocket.GetType().Name}");
			}

			return InitializeWebSocketTask = _connectAsync(_cancellationTokenSource.Token);
		}

		private IObservable<GraphQLWebSocketResponse> _createResponseStream()
		{
			return Observable.Create<GraphQLWebSocketResponse>(_createResultStream)
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
				.Catch<GraphQLWebSocketResponse, OperationCanceledException>(exception =>
					Observable.Empty<GraphQLWebSocketResponse>());
		}

		private async Task<IDisposable> _createResultStream(IObserver<GraphQLWebSocketResponse> observer, CancellationToken token)
		{
			var observable = await _getReceiveResultStream().ConfigureAwait(false);
			return new CompositeDisposable
			(
				observable.Subscribe(observer),
				Disposable.Create(() =>
				{
					Debug.WriteLine("response stream disposed");
				})
			);
		}

		private async Task<IObservable<GraphQLWebSocketResponse>> _getReceiveResultStream()
		{
			await InitializeWebSocket().ConfigureAwait(false);
			return Observable.Defer(() => _getReceiveTask().ToObservable()).Repeat();
		}

		private async Task _connectAsync(CancellationToken token)
		{
			await _backOff().ConfigureAwait(false);
			Debug.WriteLine($"opening websocket {clientWebSocket.GetHashCode()}");
			await clientWebSocket.ConnectAsync(webSocketUri, token).ConfigureAwait(false);
			Debug.WriteLine($"connection established on websocket {clientWebSocket.GetHashCode()}");
			_connectionAttempt = 1;
		}


		private Task<GraphQLWebSocketResponse> _receiveAsyncTask = null;
		private object _receiveTaskLocker = new object();
		/// <summary>
		/// wrapper method to pick up the existing request task if already running
		/// </summary>
		/// <returns></returns>
		private Task<GraphQLWebSocketResponse> _getReceiveTask()
		{
			lock (_receiveTaskLocker)
			{
				if (_receiveAsyncTask == null ||
				    _receiveAsyncTask.IsFaulted ||
				    _receiveAsyncTask.IsCompleted)
					_receiveAsyncTask = _receiveResultAsync();
			}

			return _receiveAsyncTask;
		}

		private async Task<GraphQLWebSocketResponse> _receiveResultAsync()
		{
			try
			{
				Debug.WriteLine("receiving websocket data ...");
				_cancellationTokenSource.Token.ThrowIfCancellationRequested();
				var webSocketReceiveResult =
					await clientWebSocket.ReceiveAsync(arraySegment, CancellationToken.None).ConfigureAwait(false);
				_cancellationTokenSource.Token.ThrowIfCancellationRequested();
				var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
				return JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				Debug.WriteLine("websocket data received");
			}
		}

		private async Task _closeAsync(CancellationToken cancellationToken = default)
		{
			if(clientWebSocket == null)
				return;

			// don't attempt to close the websocket if it is in a failed state
			if (this.clientWebSocket.State != WebSocketState.Open &&
			    this.clientWebSocket.State != WebSocketState.CloseReceived &&
			    this.clientWebSocket.State != WebSocketState.CloseSent)
			{
				Debug.WriteLine($"websocket {clientWebSocket.GetHashCode()} state = {this.clientWebSocket.State}");
				return;
			}

			Debug.WriteLine($"closing websocket {clientWebSocket.GetHashCode()}");
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken).ConfigureAwait(false);
		}

		#endregion

		#region IDisposable

		private Task _disposed;
		private object _disposedLocker = new object();
		public void Dispose()
		{
			// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
			lock (_disposedLocker)
			{
				if (_disposed == null) _disposed = DisposeAsync();
			}
		}

		private async Task DisposeAsync()
		{
			Debug.WriteLine($"disposing websocket {clientWebSocket.GetHashCode()}...");
			if (!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource.Cancel();
			await _closeAsync().ConfigureAwait(false);
			clientWebSocket?.Dispose();
			_cancellationTokenSource.Dispose();
			Debug.WriteLine($"websocket {clientWebSocket.GetHashCode()} disposed");
		}
		#endregion
	}
}
