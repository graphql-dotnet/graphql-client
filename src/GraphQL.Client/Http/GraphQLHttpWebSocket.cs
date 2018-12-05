using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
		private readonly Action<Exception> _webSocketExceptionHandler;
		private readonly byte[] buffer = new byte[1024 * 1024];
		private readonly ArraySegment<byte> arraySegment;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		public IObservable<GraphQLWebSocketResponse> ResponseStream { get; }

		private ClientWebSocket clientWebSocket = null;

		public GraphQLHttpWebSocket(Uri webSocketUri, Action<Exception> webSocketExceptionHandler)
		{
			this.webSocketUri = webSocketUri;
			_webSocketExceptionHandler = webSocketExceptionHandler;
			arraySegment = new ArraySegment<byte>(buffer);
			ResponseStream = _createResponseStream();
		}

		public async Task SendWebSocketRequest(GraphQLWebSocketRequest request)
		{
			await _initializeWebSocket().ConfigureAwait(false);
			var webSocketRequestString = JsonConvert.SerializeObject(request);
			var arraySegmentWebSocketRequest = new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString));
			await this.clientWebSocket.SendAsync(arraySegmentWebSocketRequest, WebSocketMessageType.Text, true, _cancellationTokenSource.Token).ConfigureAwait(false);
		}

		/// <summary>
		/// The back-off strategy for automatic reconnects. Calculates the delay before the next connection attempt is made.<br/>
		/// default formula: min(n, 5) * 1,5 * random(0.0, 1.0)
		/// </summary>
		public static Func<int, TimeSpan> BackOffStrategy = n =>
		{
			var rnd = new Random();
			return TimeSpan.FromSeconds(Math.Min(n, 5) * 1.5 + rnd.NextDouble());
		};

		#region Private Methods

		private IObservable<GraphQLWebSocketResponse> _createResponseStream()
		{
			int reconnectionAttempt = 0;

			return Observable.Create<GraphQLWebSocketResponse>(observer =>
				{
					var observable = _getReceiveResultStream().GetAwaiter().GetResult();
					// when reconnecting, apply the delay computed by the BackOffStrategy
					observable = (++reconnectionAttempt == 1)
						? observable
						: observable.DelaySubscription(BackOffStrategy(reconnectionAttempt - 1));
					return observable.Subscribe(observer);
				})
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token
				.Catch<GraphQLWebSocketResponse, OperationCanceledException>(exception =>
					Observable.Empty<GraphQLWebSocketResponse>())
				// wrap results
				.Select(response => new Tuple<GraphQLWebSocketResponse, Exception>(response, null))
				// do exception handling
				.Catch<Tuple<GraphQLWebSocketResponse, Exception>, Exception>(e =>
				{
					try
					{
						// exceptions thrown by the handler will propagate to OnError()
						_webSocketExceptionHandler?.Invoke(e);

						// throw exception on the observable to be caught by Retry() or complete sequence if cancellation was requested
						return _cancellationTokenSource.Token.IsCancellationRequested
							? Observable.Empty<Tuple<GraphQLWebSocketResponse, Exception>>()
							: Observable.Throw<Tuple<GraphQLWebSocketResponse, Exception>>(e);
					}
					catch (Exception exception)
					{
						// wrap all other exceptions to be propagated behind retry
						return Observable.Return(new Tuple<GraphQLWebSocketResponse, Exception>(null, exception));
					}
				})
				// attempt to recreate the subscription stream for rethrown exceptions
				.Retry()
				// unwrap and push results or throw wrapped exceptions
				.SelectMany(t =>
				{
					// if the result contains an exception, throw it on the observable
					if (t.Item2 != null)
						return Observable.Throw<GraphQLWebSocketResponse>(t.Item2);

					// else a value from OnNext() has arrived, so reset the reconnectionAttempt counter and pass the value on
					reconnectionAttempt = 1;

					return t.Item1 == null
						? Observable.Empty<GraphQLWebSocketResponse>()
						: Observable.Return(t.Item1);
				})
				// transform to hot observable and auto-connect
				.Publish().RefCount();
		}

		private Task _initializeWebSocketTask;
		private Task _initializeWebSocket()
		{
			// do not attempt to initialize if cancellation is requested
			if(_disposed != null)
				throw new OperationCanceledException();

			// if an initialization task is already running, return that
			if(_initializeWebSocketTask != null &&
			   !_initializeWebSocketTask.IsFaulted &&
			   !_initializeWebSocketTask.IsCompleted)
				return _initializeWebSocketTask;

			// if the websocket is open, return a completed task
			if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
				return Task.CompletedTask;

			// else (re-)create websocket and connect
			clientWebSocket?.Dispose();
			clientWebSocket = new ClientWebSocket();
			this.clientWebSocket.Options.AddSubProtocol("graphql-ws");
			return _initializeWebSocketTask = _connectAsync(_cancellationTokenSource.Token);
		}

		private async Task<IObservable<GraphQLWebSocketResponse>> _getReceiveResultStream()
		{
			await _initializeWebSocket().ConfigureAwait(false);
			return Observable.Defer(() => _receiveResultAsync().ToObservable()).Repeat();
		}

		private async Task _connectAsync(CancellationToken token)
		{
			Debug.WriteLine($"opening websocket {this.GetHashCode()}");
			await clientWebSocket.ConnectAsync(webSocketUri, token).ConfigureAwait(false);
			Debug.WriteLine($"connection established on websocket {this.GetHashCode()}");
		}

		private async Task<GraphQLWebSocketResponse> _receiveResultAsync()
		{
			try
			{
				_cancellationTokenSource.Token.ThrowIfCancellationRequested();
				var webSocketReceiveResult = await clientWebSocket.ReceiveAsync(arraySegment, CancellationToken.None).ConfigureAwait(false);
				_cancellationTokenSource.Token.ThrowIfCancellationRequested();
				var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
				return JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
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
				Debug.WriteLine($"websocket {this.GetHashCode()} state = {this.clientWebSocket.State}");
				return;
			}

			Debug.WriteLine($"closing websocket {this.GetHashCode()}");
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
			Debug.WriteLine($"disposing websocket {this.GetHashCode()}...");
			if (!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource.Cancel();
			await _closeAsync().ConfigureAwait(false);
			clientWebSocket?.Dispose();
			_cancellationTokenSource.Dispose();
			Debug.WriteLine($"websocket {this.GetHashCode()} disposed");
		}
		#endregion
	}
}
