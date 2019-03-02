using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
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
	// ReSharper disable once InconsistentNaming
	internal class GraphQLHttpWebSocket: IDisposable
	{
		private readonly Uri _webSocketUri;
		private readonly GraphQLHttpClientOptions _options;
		private readonly ArraySegment<byte> _buffer;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly Subject<GraphQLWebSocketRequest> _requestSubject = new Subject<GraphQLWebSocketRequest>();
		private readonly Subject<Exception> _exceptionSubject = new Subject<Exception>();

		private Subject<GraphQLWebSocketResponse> _responseSubject;
		private WebSocket _clientWebSocket = null;
		private int _connectionAttempt = 0;

		public IObservable<Exception> ReceiveErrors => _exceptionSubject.AsObservable();
		public IObservable<GraphQLWebSocketResponse> ResponseStream { get; }
		public WebSocketState WebSocketState => _clientWebSocket?.State ?? WebSocketState.None;

		public GraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClientOptions options)
		{
			this._webSocketUri = webSocketUri;
			_options = options;
			_buffer = new ArraySegment<byte>(new byte[8192]);
			ResponseStream = _createResponseStream();

			_requestSubject.Select(request => Observable.FromAsync(() => _sendWebSocketRequest(request))).Concat().Subscribe();
		}

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
				await this._clientWebSocket.SendAsync(
					new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString)),
					WebSocketMessageType.Text,
					true,
					_cancellationTokenSource.Token).ConfigureAwait(false);
				request.SendCompleted();
			}
			catch (Exception e)
			{
				request.SendFailed(e);
			}
		}

		private Task _initializeWebSocketTask = Task.CompletedTask;
		private readonly object _initializeLock = new object();

		/// <summary>
		/// Initializes the websocket. If the initialization process is already running, returns the running task.
		/// If a healthy websocket connection is already established, a completed task is returned
		/// </summary>
		/// <returns>the async task initializing the websocket</returns>
		public Task InitializeWebSocket()
		{
			// do not attempt to initialize if cancellation is requested
			if (_disposed != null)
				throw new OperationCanceledException();

			lock (_initializeLock)
			{
				// if an initialization task is already running, return that
				if (_initializeWebSocketTask != null &&
				    !_initializeWebSocketTask.IsFaulted &&
				    !_initializeWebSocketTask.IsCompleted)
					return _initializeWebSocketTask;

				// if the websocket is open, return a completed task
				if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
					return Task.CompletedTask;

				// else (re-)create websocket and connect
				_clientWebSocket?.Dispose();

				// fix websocket not supported on win 7 using
				// https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
				_clientWebSocket = SystemClientWebSocket.CreateClientWebSocket();
				switch (_clientWebSocket)
				{
					case ClientWebSocket nativeWebSocket:
						nativeWebSocket.Options.AddSubProtocol("graphql-ws");
						break;
					case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
						managedWebSocket.Options.AddSubProtocol("graphql-ws");
						break;
					default:
						throw new NotSupportedException($"unknown websocket type {_clientWebSocket.GetType().Name}");
				}

				return _initializeWebSocketTask = _connectAsync(_cancellationTokenSource.Token);
			}
		}

		#region Private Methods
		
		private IObservable<GraphQLWebSocketResponse> _createResponseStream()
		{
			return Observable.Create<GraphQLWebSocketResponse>(_createResultStream)
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
				.Catch<GraphQLWebSocketResponse, OperationCanceledException>(exception =>
					Observable.Empty<GraphQLWebSocketResponse>());
		}

		private async Task<IDisposable> _createResultStream(IObserver<GraphQLWebSocketResponse> observer, CancellationToken token)
		{
			if (_responseSubject == null || _responseSubject.IsDisposed)
			{
				_responseSubject = new Subject<GraphQLWebSocketResponse>();
				var observable = await _getReceiveResultStream().ConfigureAwait(false);
				observable.Subscribe(_responseSubject);

				_responseSubject.Subscribe(_ => { }, ex =>
				{
					_exceptionSubject.OnNext(ex);
					_responseSubject?.Dispose();
					_responseSubject = null;
				},
				() => {
					_responseSubject?.Dispose();
					_responseSubject = null;
				});
			}
					   
			return new CompositeDisposable
			(
				_responseSubject.Subscribe(observer),
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
			try
			{
				await _backOff().ConfigureAwait(false);
				Debug.WriteLine($"opening websocket {_clientWebSocket.GetHashCode()}");
				await _clientWebSocket.ConnectAsync(_webSocketUri, token).ConfigureAwait(false);
				Debug.WriteLine($"connection established on websocket {_clientWebSocket.GetHashCode()}");
				_connectionAttempt = 1;
			}
			catch (Exception e)
			{
				_exceptionSubject.OnNext(e);
				throw;
			}
		}

		private Task _backOff()
		{
			_connectionAttempt++;

			if (_connectionAttempt == 1) return Task.CompletedTask;

			var delay = _options.BackOffStrategy(_connectionAttempt - 1);
			Debug.WriteLine($"connection attempt #{_connectionAttempt}, backing off for {delay.TotalSeconds} s");
			return Task.Delay(delay);
		}

		private Task<GraphQLWebSocketResponse> _receiveAsyncTask = null;
		private readonly object _receiveTaskLocker = new object();

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
				Debug.WriteLine($"receiving data on websocket {_clientWebSocket.GetHashCode()} ...");

				using (var ms = new MemoryStream())
				{
					WebSocketReceiveResult webSocketReceiveResult = null;
					do
					{
						_cancellationTokenSource.Token.ThrowIfCancellationRequested();
						webSocketReceiveResult = await _clientWebSocket.ReceiveAsync(_buffer, CancellationToken.None).ConfigureAwait(false);
						ms.Write(_buffer.Array, _buffer.Offset, webSocketReceiveResult.Count);
					}
					while (!webSocketReceiveResult.EndOfMessage);

					_cancellationTokenSource.Token.ThrowIfCancellationRequested();
					ms.Seek(0, SeekOrigin.Begin);

					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
					{
						using (var reader = new StreamReader(ms, Encoding.UTF8))
						{
							var stringResult = await reader.ReadToEndAsync().ConfigureAwait(false);
							Debug.WriteLine($"data received on websocket {_clientWebSocket.GetHashCode()}: {stringResult}");
							return JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
						}
					}
					else
					{
						throw new NotSupportedException("binary websocket messages are not supported");
					}
				}				
			}
			catch (Exception e)
			{
				Debug.WriteLine($"exception thrown while receiving websocket data: {e}");
				throw;
			}
		}

		private async Task _closeAsync(CancellationToken cancellationToken = default)
		{
			if(_clientWebSocket == null)
				return;

			// don't attempt to close the websocket if it is in a failed state
			if (this._clientWebSocket.State != WebSocketState.Open &&
			    this._clientWebSocket.State != WebSocketState.CloseReceived &&
			    this._clientWebSocket.State != WebSocketState.CloseSent)
			{
				Debug.WriteLine($"websocket {_clientWebSocket.GetHashCode()} state = {this._clientWebSocket.State}");
				return;
			}

			Debug.WriteLine($"closing websocket {_clientWebSocket.GetHashCode()}");
			await this._clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken).ConfigureAwait(false);
		}

		#endregion

		#region IDisposable

		private Task _disposed;
		private readonly object _disposedLocker = new object();
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
			Debug.WriteLine($"disposing websocket {_clientWebSocket?.GetHashCode()}...");
			if (!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource.Cancel();
			await _closeAsync().ConfigureAwait(false);
			_clientWebSocket?.Dispose();
			_cancellationTokenSource.Dispose();
			Debug.WriteLine($"websocket {_clientWebSocket?.GetHashCode()} disposed");
		}
		#endregion
	}
}
