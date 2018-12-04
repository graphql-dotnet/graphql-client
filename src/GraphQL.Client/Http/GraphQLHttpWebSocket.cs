using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http
{
	internal class GraphQLHttpWebSocket: IDisposable
	{
		private readonly Uri webSocketUri;
		private readonly byte[] buffer = new byte[1024 * 1024];
		private readonly ArraySegment<byte> arraySegment;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private ClientWebSocket clientWebSocket = null;

		public GraphQLHttpWebSocket(Uri webSocketUri)
		{
			this.webSocketUri = webSocketUri;
			arraySegment = new ArraySegment<byte>(buffer);
		}

		public Task SendWebSocketRequest(GraphQLWebSocketRequest request)
		{
			if (clientWebSocket == null)
			{
				throw new InvalidOperationException("websocket not connected! subscribe to the response stream before sending a request!");
			}
			var webSocketRequestString = JsonConvert.SerializeObject(request);
			var arraySegmentWebSocketRequest = new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString));
			return this.clientWebSocket.SendAsync(arraySegmentWebSocketRequest, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
		}

		public IObservable<GraphQLWebSocketResponse> ResponseStream
		{
			get
			{
				int reconnectionAttempt = 0;

				return Observable
					// create deferred observable using a GraphQLHttpObservableSubscription instance
					.Defer(
						() =>
						{
							var observable = Observable.Using(
								token => _createClientWebsocket(),
								(_, cancellationToken) => _initializeWebSocketConnection(cancellationToken));

							// when reconnecting, apply the delay computed by the BackOffStrategy
							return (++reconnectionAttempt == 1)
								? observable
								: observable.DelaySubscription(BackOffStrategy(reconnectionAttempt - 1));
						}
					)
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
							_handleExceptions(e);

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
						return Observable.Return(t.Item1);
					})
					// transform to hot observable and auto-connect
					.Publish().RefCount();
			}
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

		private void _handleExceptions(Exception e)
		{

		}

		private Task<IDisposable> _createClientWebsocket()
		{
			clientWebSocket = new ClientWebSocket();
			this.clientWebSocket.Options.AddSubProtocol("graphql-ws");

			return Task.FromResult(Disposable.Create(() => clientWebSocket?.Dispose()));
		}

		private async Task<IObservable<GraphQLWebSocketResponse>> _initializeWebSocketConnection(CancellationToken cancelToken)
		{
			await _connectAsync(cancelToken).ConfigureAwait(false);
			return Observable.Defer(() => _receiveResultAsync().ToObservable()).Repeat();
		}

		private async Task _connectAsync(CancellationToken token)
		{
			Debug.WriteLine($"opening websocket on subscription {this.GetHashCode()}");
			await clientWebSocket.ConnectAsync(webSocketUri, token).ConfigureAwait(false);
			Debug.WriteLine($"connection established on subscription {this.GetHashCode()}");
		}

		private async Task<GraphQLWebSocketResponse> _receiveResultAsync()
		{
			var webSocketReceiveResult = await clientWebSocket.ReceiveAsync(arraySegment, _cancellationTokenSource.Token).ConfigureAwait(false);
			var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
			return JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
		}

		private async Task _closeAsync(CancellationToken cancellationToken = default)
		{
			// don't attempt to close the websocket if it is in a failed state
			if (this.clientWebSocket.State != WebSocketState.Open &&
			    this.clientWebSocket.State != WebSocketState.CloseReceived &&
			    this.clientWebSocket.State != WebSocketState.CloseSent)
				return;

			Debug.WriteLine($"closing websocket {this.GetHashCode()}");
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken).ConfigureAwait(false);
		}

		#endregion

		#region IDisposable

		private Task Disposed { get; set; }
		private object _disposedLocker = new object();
		public void Dispose()
		{
			// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
			lock (_disposedLocker)
			{
				if (Disposed == null) Disposed = DisposeAsync();
			}
		}

		private async Task DisposeAsync()
		{
			Debug.WriteLine($"disposing subscription {this.GetHashCode()}...");
			if (!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource.Cancel();
			await _closeAsync().ConfigureAwait(false);
			clientWebSocket?.Dispose();
			_cancellationTokenSource.Dispose();
			Debug.WriteLine($"subscription {this.GetHashCode()} disposed");
		}
		#endregion
	}
}
