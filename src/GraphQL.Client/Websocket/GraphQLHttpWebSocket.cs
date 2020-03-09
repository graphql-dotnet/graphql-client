using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket {
	internal class GraphQLHttpWebSocket : IDisposable {

		#region Private fields

		private readonly Uri webSocketUri;
		private readonly GraphQLHttpClient client;
		private readonly ArraySegment<byte> buffer;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly CancellationToken cancellationToken;
		private readonly Subject<GraphQLWebSocketRequest> requestSubject = new Subject<GraphQLWebSocketRequest>();
		private readonly Subject<Exception> exceptionSubject = new Subject<Exception>();
		private readonly BehaviorSubject<GraphQLWebsocketConnectionState> stateSubject =
			new BehaviorSubject<GraphQLWebsocketConnectionState>(GraphQLWebsocketConnectionState.Disconnected);
		private readonly IDisposable requestSubscription;
		private readonly EventLoopScheduler receiveLoopScheduler = new EventLoopScheduler();
		private readonly EventLoopScheduler sendLoopScheduler = new EventLoopScheduler();

		private int connectionAttempt = 0;
		private Subject<WebsocketMessageWrapper> incomingMessagesSubject;
		private IDisposable incomingMessagesDisposable;
		private GraphQLHttpClientOptions Options => client.Options;

#if NETFRAMEWORK
		private WebSocket clientWebSocket = null;
#else
		private ClientWebSocket clientWebSocket = null;
#endif

		#endregion
		
		public WebSocketState WebSocketState => clientWebSocket?.State ?? WebSocketState.None;
		public IObservable<Exception> ReceiveErrors => exceptionSubject.AsObservable();
		public IObservable<GraphQLWebsocketConnectionState> ConnectionState => stateSubject.DistinctUntilChanged();
		public IObservable<WebsocketMessageWrapper> ResponseStream { get; }

		public GraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClient client) {
			cancellationToken = cancellationTokenSource.Token;
			this.webSocketUri = webSocketUri;
			this.client = client;
			buffer = new ArraySegment<byte>(new byte[8192]);
			ResponseStream = GetMessageStream();
			receiveLoopScheduler.Schedule(() =>
				Debug.WriteLine($"receive loop scheduler thread id: {Thread.CurrentThread.ManagedThreadId}"));

			requestSubscription = requestSubject
				.ObserveOn(sendLoopScheduler)
				.Subscribe(async request => await SendWebSocketRequest(request));
		}
		
		#region Send requests

		public Task QueueWebSocketRequest(GraphQLWebSocketRequest request) {
			requestSubject.OnNext(request);
			return request.SendTask();
		}

		private async Task SendWebSocketRequest(GraphQLWebSocketRequest request) {
			try {
				if (cancellationToken.IsCancellationRequested) {
					request.SendCanceled();
					return;
				}

				await InitializeWebSocket();
				var requestBytes = Options.JsonSerializer.SerializeToBytes(request);
				await this.clientWebSocket.SendAsync(
					new ArraySegment<byte>(requestBytes),
					WebSocketMessageType.Text,
					true,
					cancellationToken);
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
				clientWebSocket?.Dispose();

#if NETFRAMEWORK
				// fix websocket not supported on win 7 using
				// https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
				clientWebSocket = SystemClientWebSocket.CreateClientWebSocket();
				switch (clientWebSocket) {
					case ClientWebSocket nativeWebSocket:
						nativeWebSocket.Options.AddSubProtocol("graphql-ws");
						nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
						nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
						break;
					case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
						managedWebSocket.Options.AddSubProtocol("graphql-ws");
						managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
						managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
						break;
					default:
						throw new NotSupportedException($"unknown websocket type {clientWebSocket.GetType().Name}");
				}
#else
				clientWebSocket = new ClientWebSocket();
				clientWebSocket.Options.AddSubProtocol("graphql-ws");
				clientWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
				clientWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
#endif
				return initializeWebSocketTask = ConnectAsync(cancellationToken);
			}
		}

		private async Task ConnectAsync(CancellationToken token) {
			try {
				await BackOff();
				stateSubject.OnNext(GraphQLWebsocketConnectionState.Connecting);
				Debug.WriteLine($"opening websocket {clientWebSocket.GetHashCode()}");
				await clientWebSocket.ConnectAsync(webSocketUri, token);
				stateSubject.OnNext(GraphQLWebsocketConnectionState.Connected);
				Debug.WriteLine($"connection established on websocket {clientWebSocket.GetHashCode()}, invoking Options.OnWebsocketConnected()");
				await (Options.OnWebsocketConnected?.Invoke(client) ?? Task.CompletedTask);
				Debug.WriteLine($"invoking Options.OnWebsocketConnected() on websocket {clientWebSocket.GetHashCode()}");
				connectionAttempt = 1;
			}
			catch (Exception e) {
				stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
				exceptionSubject.OnNext(e);
				throw;
			}
		}

		/// <summary>
		/// delay the next connection attempt using <see cref="GraphQLHttpClientOptions.BackOffStrategy"/>
		/// </summary>
		/// <returns></returns>
		private Task BackOff() {
			connectionAttempt++;

			if (connectionAttempt == 1) return Task.CompletedTask;

			var delay = Options.BackOffStrategy?.Invoke(connectionAttempt - 1) ?? TimeSpan.FromSeconds(5);
			Debug.WriteLine($"connection attempt #{connectionAttempt}, backing off for {delay.TotalSeconds} s");
			return Task.Delay(delay, cancellationToken);
		}


		private IObservable<WebsocketMessageWrapper> GetMessageStream() {
			return Observable.Create<WebsocketMessageWrapper>(CreateMessageStream)
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
				.Catch<WebsocketMessageWrapper, OperationCanceledException>(exception =>
					Observable.Empty<WebsocketMessageWrapper>());
		}

		private async Task<IDisposable> CreateMessageStream(IObserver<WebsocketMessageWrapper> observer, CancellationToken token) {
			var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
			cts.Token.ThrowIfCancellationRequested();


			if (incomingMessagesSubject == null || incomingMessagesSubject.IsDisposed) {
				// create new response subject
				incomingMessagesSubject = new Subject<WebsocketMessageWrapper>();
				Debug.WriteLine($"creating new incoming message stream {incomingMessagesSubject.GetHashCode()}");

				// initialize and connect websocket
				await InitializeWebSocket();

				// loop the receive task and subscribe the created subject to the results 
				var receiveLoopSubscription = Observable
					.Defer(() => GetReceiveTask().ToObservable())
					.Repeat()
					.Subscribe(incomingMessagesSubject);
				
				incomingMessagesDisposable = new CompositeDisposable(
					incomingMessagesSubject,
					receiveLoopSubscription,
					Disposable.Create(() => {
						Debug.WriteLine($"incoming message stream {incomingMessagesSubject.GetHashCode()} disposed");
					}));

				// dispose the subject on any error or completion (will be recreated) 
				incomingMessagesSubject.Subscribe(_ => { }, ex => {
					exceptionSubject.OnNext(ex);
					incomingMessagesDisposable?.Dispose();
					incomingMessagesSubject = null;
					stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
				},
				() => {
					incomingMessagesDisposable?.Dispose();
					incomingMessagesSubject = null;
					stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
				});
			}

			var subscription = new CompositeDisposable(incomingMessagesSubject.Subscribe(observer));
			var hashCode = subscription.GetHashCode();
			subscription.Add(Disposable.Create(() => {
				Debug.WriteLine($"incoming message subscription {hashCode} disposed");
			}));
			Debug.WriteLine($"new incoming message subscription {hashCode} created");
			return subscription;
		}

		private Task<WebsocketMessageWrapper> receiveAsyncTask = null;
		private readonly object receiveTaskLocker = new object();
		/// <summary>
		/// wrapper method to pick up the existing request task if already running
		/// </summary>
		/// <returns></returns>
		private Task<WebsocketMessageWrapper> GetReceiveTask() {
			lock (receiveTaskLocker) {
				cancellationToken.ThrowIfCancellationRequested();
				if (receiveAsyncTask == null ||
					receiveAsyncTask.IsFaulted ||
					receiveAsyncTask.IsCompleted)
					receiveAsyncTask = ReceiveWebsocketMessagesAsync();
			}

			return receiveAsyncTask;
		}

		/// <summary>
		/// read a single message from the websocket
		/// </summary>
		/// <returns></returns>
		private async Task<WebsocketMessageWrapper> ReceiveWebsocketMessagesAsync() {
			try {
				Debug.WriteLine($"waiting for data on websocket {clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})...");

				using (var ms = new MemoryStream()) {
					WebSocketReceiveResult webSocketReceiveResult = null;
					do {
						cancellationToken.ThrowIfCancellationRequested();
						webSocketReceiveResult = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
						ms.Write(buffer.Array, buffer.Offset, webSocketReceiveResult.Count);
					}
					while (!webSocketReceiveResult.EndOfMessage);

					cancellationToken.ThrowIfCancellationRequested();
					ms.Seek(0, SeekOrigin.Begin);

					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text) {
						var response = await Options.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms);
						response.MessageBytes = ms.ToArray();
						Debug.WriteLine($"{response.MessageBytes.Length} bytes received on websocket {clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})...");
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

		private async Task CloseAsync() {
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
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
			stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
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
			await CloseAsync();
			requestSubscription?.Dispose();
			clientWebSocket?.Dispose();

			incomingMessagesSubject?.OnCompleted();
			incomingMessagesDisposable?.Dispose();

			stateSubject?.OnCompleted();
			stateSubject?.Dispose();

			exceptionSubject?.OnCompleted();
			exceptionSubject?.Dispose();
			cancellationTokenSource.Dispose();

			sendLoopScheduler?.Dispose();
			receiveLoopScheduler?.Dispose();

			Debug.WriteLine($"websocket {clientWebSocket.GetHashCode()} disposed");
		}
#endregion
	}
}
