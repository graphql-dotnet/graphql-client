using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket {
	internal class GraphQLHttpWebSocket : IDisposable {
		private readonly Uri webSocketUri;
		private readonly GraphQLHttpClientOptions options;
		private readonly ArraySegment<byte> buffer;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly Subject<GraphQLWebSocketRequest> requestSubject = new Subject<GraphQLWebSocketRequest>();
		private readonly Subject<Exception> exceptionSubject = new Subject<Exception>();
		private readonly IDisposable requestSubscription;

		private int connectionAttempt = 0;
		private Subject<WebsocketResponseWrapper> responseSubject;

#if NETFRAMEWORK
		private WebSocket clientWebSocket = null;
#else
		private ClientWebSocket clientWebSocket = null;
#endif


		public WebSocketState WebSocketState => clientWebSocket?.State ?? WebSocketState.None;
		public IObservable<Exception> ReceiveErrors => exceptionSubject.AsObservable();
		public IObservable<WebsocketResponseWrapper> ResponseStream { get; }

		public GraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClientOptions options) {
			this.webSocketUri = webSocketUri;
			this.options = options;
			buffer = new ArraySegment<byte>(new byte[8192]);
			ResponseStream = _createResponseStream();

			requestSubscription = requestSubject.Select(request => Observable.FromAsync(() => _sendWebSocketRequest(request))).Concat().Subscribe();
		}


		#region Send requests

		public Task SendWebSocketRequest(GraphQLWebSocketRequest request) {
			requestSubject.OnNext(request);
			return request.SendTask();
		}

		private async Task _sendWebSocketRequest(GraphQLWebSocketRequest request) {
			try {
				if (cancellationTokenSource.Token.IsCancellationRequested) {
					request.SendCanceled();
					return;
				}

				await InitializeWebSocket().ConfigureAwait(false);
				var requestBytes = options.JsonSerializer.SerializeToBytes(request);
				await this.clientWebSocket.SendAsync(
					new ArraySegment<byte>(requestBytes),
					WebSocketMessageType.Text,
					true,
					cancellationTokenSource.Token).ConfigureAwait(false);
				request.SendCompleted();
			}
			catch (Exception e) {
				request.SendFailed(e);
			}
		}

		#endregion

		private Task initializeWebSocketTask = Task.CompletedTask;
		private readonly object initializeLock = new object();
		
		public Task InitializeWebSocket() {
			// do not attempt to initialize if cancellation is requested
			if (Completion != null)
				throw new OperationCanceledException();

			lock (initializeLock) {
				// if an initialization task is already running, return that
				if (initializeWebSocketTask != null &&
				   !initializeWebSocketTask.IsFaulted &&
				   !initializeWebSocketTask.IsCompleted)
					return initializeWebSocketTask;

				// if the websocket is open, return a completed task
				if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
					return Task.CompletedTask;

				// else (re-)create websocket and connect
				//_responseStreamConnection?.Dispose();
				clientWebSocket?.Dispose();

#if NETFRAMEWORK
				// fix websocket not supported on win 7 using
				// https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
				clientWebSocket = SystemClientWebSocket.CreateClientWebSocket();
				switch (clientWebSocket) {
					case ClientWebSocket nativeWebSocket:
						nativeWebSocket.Options.AddSubProtocol("graphql-ws");
						nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)options.HttpMessageHandler).ClientCertificates;
						nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)options.HttpMessageHandler).UseDefaultCredentials;
						break;
					case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
						managedWebSocket.Options.AddSubProtocol("graphql-ws");
						managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)options.HttpMessageHandler).ClientCertificates;
						managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)options.HttpMessageHandler).UseDefaultCredentials;
						break;
					default:
						throw new NotSupportedException($"unknown websocket type {clientWebSocket.GetType().Name}");
				}
#else
				clientWebSocket = new ClientWebSocket();
				clientWebSocket.Options.AddSubProtocol("graphql-ws");
				clientWebSocket.Options.ClientCertificates = ((HttpClientHandler)options.HttpMessageHandler).ClientCertificates;
				clientWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)options.HttpMessageHandler).UseDefaultCredentials;
#endif
				return initializeWebSocketTask = _connectAsync(cancellationTokenSource.Token);
			}
		}

		private async Task _connectAsync(CancellationToken token) {
			try {
				await _backOff().ConfigureAwait(false);
				Debug.WriteLine($"opening websocket {clientWebSocket.GetHashCode()}");
				await clientWebSocket.ConnectAsync(webSocketUri, token).ConfigureAwait(false);
				Debug.WriteLine($"connection established on websocket {clientWebSocket.GetHashCode()}");
				connectionAttempt = 1;
			}
			catch (Exception e) {
				exceptionSubject.OnNext(e);
				throw;
			}
		}

		/// <summary>
		/// delay the next connection attempt using <see cref="GraphQLHttpClientOptions.BackOffStrategy"/>
		/// </summary>
		/// <returns></returns>
		private Task _backOff() {
			connectionAttempt++;

			if (connectionAttempt == 1) return Task.CompletedTask;

			var delay = options.BackOffStrategy?.Invoke(connectionAttempt - 1) ?? TimeSpan.FromSeconds(5);
			Debug.WriteLine($"connection attempt #{connectionAttempt}, backing off for {delay.TotalSeconds} s");
			return Task.Delay(delay);
		}


		private IObservable<WebsocketResponseWrapper> _createResponseStream() {
			return Observable.Create<WebsocketResponseWrapper>(_createResultStream)
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
				.Catch<WebsocketResponseWrapper, OperationCanceledException>(exception =>
					Observable.Empty<WebsocketResponseWrapper>());
		}

		private async Task<IDisposable> _createResultStream(IObserver<WebsocketResponseWrapper> observer, CancellationToken token) {
			if (responseSubject == null || responseSubject.IsDisposed) {
				// create new response subject
				responseSubject = new Subject<WebsocketResponseWrapper>();

				// initialize and connect websocket
				await InitializeWebSocket().ConfigureAwait(false);

				// loop the receive task and subscribe the created subject to the results 
				Observable.Defer(() => _getReceiveTask().ToObservable()).Repeat().Subscribe(responseSubject);

				// dispose the subject on any error or completion (will be recreated) 
				responseSubject.Subscribe(_ => { }, ex => {
					exceptionSubject.OnNext(ex);
					responseSubject?.Dispose();
					responseSubject = null;
				},
				() => {
					responseSubject?.Dispose();
					responseSubject = null;
				});
			}

			return new CompositeDisposable
			(
				responseSubject.Subscribe(observer),
				Disposable.Create(() => {
					Debug.WriteLine("response stream disposed");
				})
			);
		}

		private Task<WebsocketResponseWrapper> receiveAsyncTask = null;
		private readonly object receiveTaskLocker = new object();
		/// <summary>
		/// wrapper method to pick up the existing request task if already running
		/// </summary>
		/// <returns></returns>
		private Task<WebsocketResponseWrapper> _getReceiveTask() {
			lock (receiveTaskLocker) {
				if (receiveAsyncTask == null ||
					receiveAsyncTask.IsFaulted ||
					receiveAsyncTask.IsCompleted)
					receiveAsyncTask = _receiveResultAsync();
			}

			return receiveAsyncTask;
		}

		/// <summary>
		/// read a single message from the websocket
		/// </summary>
		/// <returns></returns>
		private async Task<WebsocketResponseWrapper> _receiveResultAsync() {
			try {
				Debug.WriteLine($"receiving data on websocket {clientWebSocket.GetHashCode()} ...");

				using (var ms = new MemoryStream()) {
					WebSocketReceiveResult webSocketReceiveResult = null;
					do {
						cancellationTokenSource.Token.ThrowIfCancellationRequested();
						webSocketReceiveResult = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
						ms.Write(buffer.Array, buffer.Offset, webSocketReceiveResult.Count);
					}
					while (!webSocketReceiveResult.EndOfMessage);

					cancellationTokenSource.Token.ThrowIfCancellationRequested();
					ms.Seek(0, SeekOrigin.Begin);

					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text) {
						var response = await options.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms);
						response.MessageBytes = ms.ToArray();
						return response;
					}
					else {
						throw new NotSupportedException("binary websocket messages are not supported");
					}
				}
			}
			catch (Exception e) {
				Debug.WriteLine($"exception thrown while receiving websocket data: {e}");
				throw;
			}
		}

		private async Task _closeAsync(CancellationToken cancellationToken = default) {
			if (clientWebSocket == null)
				return;

			// don't attempt to close the websocket if it is in a failed state
			if (this.clientWebSocket.State != WebSocketState.Open &&
				this.clientWebSocket.State != WebSocketState.CloseReceived &&
				this.clientWebSocket.State != WebSocketState.CloseSent) {
				Debug.WriteLine($"websocket {clientWebSocket.GetHashCode()} state = {this.clientWebSocket.State}");
				return;
			}

			Debug.WriteLine($"closing websocket {clientWebSocket.GetHashCode()}");
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken).ConfigureAwait(false);
		}

		#region IDisposable
		public void Dispose() => Complete();

		/// <summary>
		/// Cancels the current operation, closes the websocket connection and disposes of internal resources.
		/// </summary>
		public void Complete() {
			lock (completedLocker) {
				if (Completion == null) Completion = CompleteAsync();
			}
		}

		/// <summary>
		/// Task to await the completion (a.k.a. disposal) of this websocket.
		/// </summary> 
		/// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
		public Task Completion { get; private set; }

		private readonly object completedLocker = new object();
		private async Task CompleteAsync() {
			Debug.WriteLine($"disposing websocket {clientWebSocket.GetHashCode()}...");
			if (!cancellationTokenSource.IsCancellationRequested)
				cancellationTokenSource.Cancel();
			await _closeAsync().ConfigureAwait(false);
			requestSubscription?.Dispose();
			clientWebSocket?.Dispose();
			cancellationTokenSource.Dispose();
			Debug.WriteLine($"websocket {clientWebSocket.GetHashCode()} disposed");
		}
#endregion
	}
}
