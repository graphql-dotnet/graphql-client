using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket
{
    internal class GraphQLHttpWebSocket : IDisposable
    {

        #region Private fields

        private readonly Uri _webSocketUri;
        private readonly GraphQLHttpClient _client;
        private readonly ArraySegment<byte> _buffer;
        private readonly CancellationTokenSource _internalCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _internalCancellationToken;
        private readonly Subject<GraphQLWebSocketRequest> _requestSubject = new Subject<GraphQLWebSocketRequest>();
        private readonly Subject<Exception> _exceptionSubject = new Subject<Exception>();
        private readonly BehaviorSubject<GraphQLWebsocketConnectionState> _stateSubject =
            new BehaviorSubject<GraphQLWebsocketConnectionState>(GraphQLWebsocketConnectionState.Disconnected);
        private readonly IDisposable _requestSubscription;

        private int _connectionAttempt = 0;
        private IConnectableObservable<WebsocketMessageWrapper> _incomingMessages;
        private IDisposable _incomingMessagesConnection;
        private GraphQLHttpClientOptions Options => _client.Options;

        private Task _initializeWebSocketTask = Task.CompletedTask;
        private readonly object _initializeLock = new object();

#if NETFRAMEWORK
		private WebSocket _clientWebSocket = null;
#else
        private ClientWebSocket _clientWebSocket = null;
#endif

        #endregion

        #region Public properties

        /// <summary>
        /// The current websocket state
        /// </summary>
        public WebSocketState WebSocketState => _clientWebSocket?.State ?? WebSocketState.None;

        /// <summary>
        /// Publishes all errors which occur within the receive pipeline
        /// </summary>
        public IObservable<Exception> ReceiveErrors => _exceptionSubject.AsObservable();

        /// <summary>
        /// Publishes the connection state of the <see cref="GraphQLHttpWebSocket"/>
        /// </summary>
        public IObservable<GraphQLWebsocketConnectionState> ConnectionState => _stateSubject.DistinctUntilChanged();

        /// <summary>
        /// Publishes all messages which are received on the websocket
        /// </summary>
        public IObservable<WebsocketMessageWrapper> IncomingMessageStream { get; }

        #endregion

        public GraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClient client)
        {
            _internalCancellationToken = _internalCancellationTokenSource.Token;
            _webSocketUri = webSocketUri;
            _client = client;
            _buffer = new ArraySegment<byte>(new byte[8192]);
            IncomingMessageStream = GetMessageStream();

            _requestSubscription = _requestSubject
                .Select(request => Observable.FromAsync(() => SendWebSocketRequestAsync(request)))
                .Concat()
                .Subscribe();
        }

        #region Send requests

        /// <summary>
        /// Create a new subscription stream
        /// </summary>
        /// <typeparam name="TResponse">the response type</typeparam>
        /// <param name="request">the <see cref="GraphQLRequest"/> to start the subscription</param>
        /// <param name="exceptionHandler">Optional: exception handler for handling exceptions within the receive pipeline</param>
        /// <returns>a <see cref="IObservable{TResponse}"/> which represents the subscription</returns>
        public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler = null) =>
            Observable.Defer(() =>
                    Observable.Create<GraphQLResponse<TResponse>>(async observer =>
                    {
                        Debug.WriteLine($"Create observable thread id: {Thread.CurrentThread.ManagedThreadId}");
                        var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client);

                        var startRequest = new GraphQLWebSocketRequest
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Type = GraphQLWebSocketMessageType.GQL_START,
                            Payload = preprocessedRequest
                        };
                        var stopRequest = new GraphQLWebSocketRequest
                        {
                            Id = startRequest.Id,
                            Type = GraphQLWebSocketMessageType.GQL_STOP
                        };

                        var observable = Observable.Create<GraphQLResponse<TResponse>>(o =>
                            IncomingMessageStream
                                // ignore null values and messages for other requests
                                .Where(response => response != null && response.Id == startRequest.Id)
                                .Subscribe(response =>
                                    {
                                        // terminate the sequence when a 'complete' message is received
                                        if (response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
                                        {
                                            Debug.WriteLine($"received 'complete' message on subscription {startRequest.Id}");
                                            o.OnCompleted();
                                            return;
                                        }

                                        // post the GraphQLResponse to the stream (even if a GraphQL error occurred)
                                        Debug.WriteLine($"received payload on subscription {startRequest.Id} (thread {Thread.CurrentThread.ManagedThreadId})");
                                        var typedResponse =
                                            _client.JsonSerializer.DeserializeToWebsocketResponse<TResponse>(
                                                response.MessageBytes);
                                        Debug.WriteLine($"payload => {System.Text.Encoding.UTF8.GetString(response.MessageBytes)}");
                                        o.OnNext(typedResponse.Payload);

                                        // in case of a GraphQL error, terminate the sequence after the response has been posted
                                        if (response.Type == GraphQLWebSocketMessageType.GQL_ERROR)
                                        {
                                            Debug.WriteLine($"terminating subscription {startRequest.Id} because of a GraphQL error");
                                            o.OnCompleted();
                                        }
                                    },
                                    e =>
                                    {
                                        Debug.WriteLine($"response stream for subscription {startRequest.Id} failed: {e}");
                                        o.OnError(e);
                                    },
                                    () =>
                                    {
                                        Debug.WriteLine($"response stream for subscription {startRequest.Id} completed");
                                        o.OnCompleted();
                                    })
                        );

                        try
                        {
                            // initialize websocket (completes immediately if socket is already open)
                            await InitializeWebSocket();
                        }
                        catch (Exception e)
                        {
                            // subscribe observer to failed observable
                            return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
                        }

                        var disposable = new CompositeDisposable(
                            observable.Subscribe(observer),
                            Disposable.Create(async () =>
                            {
                                // only try to send close request on open websocket
                                if (WebSocketState != WebSocketState.Open)
                                    return;

                                try
                                {
                                    Debug.WriteLine($"sending stop message on subscription {startRequest.Id}");
                                    await QueueWebSocketRequest(stopRequest);
                                }
                                // do not break on disposing
                                catch (OperationCanceledException) { }
                            })
                        );
                        
                        Debug.WriteLine($"sending start message on subscription {startRequest.Id}");
                        // send subscription request
                        try
                        {
                            await QueueWebSocketRequest(startRequest);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                            throw;
                        }

                        return disposable;
                    }))
                // complete sequence on OperationCanceledException, this is triggered by the cancellation token
                .Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
                    Observable.Empty<GraphQLResponse<TResponse>>())
                // wrap results
                .Select(response => new Tuple<GraphQLResponse<TResponse>, Exception>(response, null))
                // do exception handling
                .Catch<Tuple<GraphQLResponse<TResponse>, Exception>, Exception>(e =>
                {
                    try
                    {
                        if (exceptionHandler == null)
                        {
                            // if the external handler is not set, propagate all exceptions except WebSocketExceptions
                            // this will ensure that the client tries to re-establish subscriptions on connection loss
                            if (!(e is WebSocketException))
                                throw e;
                        }
                        else
                        {
                            // exceptions thrown by the handler will propagate to OnError()
                            exceptionHandler?.Invoke(e);
                        }

                        // throw exception on the observable to be caught by Retry() or complete sequence if cancellation was requested
                        if (_internalCancellationToken.IsCancellationRequested)
                            return Observable.Empty<Tuple<GraphQLResponse<TResponse>, Exception>>();
                        else
                        {
                            Debug.WriteLine($"Catch handler thread id: {Thread.CurrentThread.ManagedThreadId}");
                            return Observable.Throw<Tuple<GraphQLResponse<TResponse>, Exception>>(e);
                        }
                    }
                    catch (Exception exception)
                    {
                        // wrap all other exceptions to be propagated behind retry
                        return Observable.Return(new Tuple<GraphQLResponse<TResponse>, Exception>(null, exception));
                    }
                })
                // attempt to recreate the websocket for rethrown exceptions
                .Retry()
                // unwrap and push results or throw wrapped exceptions
                .SelectMany(t =>
                {
                    // if the result contains an exception, throw it on the observable
                    if (t.Item2 != null)
                    {
                        Debug.WriteLine($"unwrap exception thread id: {Thread.CurrentThread.ManagedThreadId} => {t.Item2}");
                        return Observable.Throw<GraphQLResponse<TResponse>>(t.Item2);
                    }
                    if (t.Item1 == null)
                    {
                        Debug.WriteLine($"empty item thread id: {Thread.CurrentThread.ManagedThreadId}");
                        return Observable.Empty<GraphQLResponse<TResponse>>();
                    }
                    return Observable.Return(t.Item1);
                })
                // transform to hot observable and auto-connect
                .Publish().RefCount();

        /// <summary>
        /// Send a regular GraphQL request (query, mutation) via websocket
        /// </summary>
        /// <typeparam name="TResponse">the response type</typeparam>
        /// <param name="request">the <see cref="GraphQLRequest"/> to send</param>
        /// <param name="cancellationToken">the token to cancel the request</param>
        /// <returns></returns>
        public Task<GraphQLResponse<TResponse>> SendRequest<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) =>
            Observable.Create<GraphQLResponse<TResponse>>(async observer =>
                {
                    await _client.Options.PreprocessRequest(request, _client);
                    var websocketRequest = new GraphQLWebSocketRequest
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Type = GraphQLWebSocketMessageType.GQL_START,
                        Payload = request
                    };
                    var observable = IncomingMessageStream
                        .Where(response => response != null && response.Id == websocketRequest.Id)
                        .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
                        .Select(response =>
                        {
                            Debug.WriteLine($"received response for request {websocketRequest.Id}");
                            var typedResponse =
                                _client.JsonSerializer.DeserializeToWebsocketResponse<TResponse>(
                                    response.MessageBytes);
                            return typedResponse.Payload;
                        });

                    try
                    {
                        // initialize websocket (completes immediately if socket is already open)
                        await InitializeWebSocket();
                    }
                    catch (Exception e)
                    {
                        // subscribe observer to failed observable
                        return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
                    }

                    var disposable = new CompositeDisposable(
                        observable.Subscribe(observer)
                    );

                    Debug.WriteLine($"submitting request {websocketRequest.Id}");
                    // send request
                    try
                    {
                        await QueueWebSocketRequest(websocketRequest);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }

                    return disposable;
                })
                // complete sequence on OperationCanceledException, this is triggered by the cancellation token
                .Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
                    Observable.Empty<GraphQLResponse<TResponse>>())
                .FirstAsync()
                .ToTask(cancellationToken);

        private Task QueueWebSocketRequest(GraphQLWebSocketRequest request)
        {
            _requestSubject.OnNext(request);
            return request.SendTask();
        }

        private async Task<Unit> SendWebSocketRequestAsync(GraphQLWebSocketRequest request)
        {
            try
            {
                if (_internalCancellationToken.IsCancellationRequested)
                {
                    request.SendCanceled();
                    return Unit.Default;
                }

                await InitializeWebSocket();
                await SendWebSocketMessageAsync(request, _internalCancellationToken);
                request.SendCompleted();
            }
            catch (Exception e)
            {
                request.SendFailed(e);
            }
            return Unit.Default;
        }

        private async Task SendWebSocketMessageAsync(GraphQLWebSocketRequest request, CancellationToken cancellationToken = default)
        {
            var requestBytes = _client.JsonSerializer.SerializeToBytes(request);
            await _clientWebSocket.SendAsync(
                new ArraySegment<byte>(requestBytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }

        #endregion

        public Task InitializeWebSocket()
        {
            // do not attempt to initialize if cancellation is requested
            if (Completion != null)
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

#if NETFRAMEWORK
				// fix websocket not supported on win 7 using
				// https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
				_clientWebSocket = SystemClientWebSocket.CreateClientWebSocket();
				switch (_clientWebSocket) {
					case ClientWebSocket nativeWebSocket:
						nativeWebSocket.Options.AddSubProtocol("graphql-ws");
						nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
						nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                        Options.ConfigureWebsocketOptions(nativeWebSocket.Options);
                        break;
					case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
						managedWebSocket.Options.AddSubProtocol("graphql-ws");
						managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
						managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                        break;
					default:
						throw new NotSupportedException($"unknown websocket type {_clientWebSocket.GetType().Name}");
				}
#else
                _clientWebSocket = new ClientWebSocket();
                _clientWebSocket.Options.AddSubProtocol("graphql-ws");

                // the following properties are not supported in Blazor WebAssembly and throw a PlatformNotSupportedException error when accessed
                try
                {
                    _clientWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                }
                catch (NotImplementedException)
                {
                    Debug.WriteLine("property 'ClientWebSocketOptions.ClientCertificates' not implemented by current platform");                
                }
                catch (PlatformNotSupportedException)
                {
                    Debug.WriteLine("property 'ClientWebSocketOptions.ClientCertificates' not supported by current platform");
                }

                try
                {
                    _clientWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                }
                catch (NotImplementedException)
                {
                    Debug.WriteLine("property 'ClientWebSocketOptions.UseDefaultCredentials' not implemented by current platform");    
                }
                catch (PlatformNotSupportedException)
                {
                    Debug.WriteLine("Property 'ClientWebSocketOptions.UseDefaultCredentials' not supported by current platform");
                }

                Options.ConfigureWebsocketOptions(_clientWebSocket.Options);
#endif
                return _initializeWebSocketTask = ConnectAsync(_internalCancellationToken);
            }
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            try
            {
                await BackOff();
                _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connecting);
                Debug.WriteLine($"opening websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})");
                await _clientWebSocket.ConnectAsync(_webSocketUri, token);
                _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connected);
                Debug.WriteLine($"connection established on websocket {_clientWebSocket.GetHashCode()}, invoking Options.OnWebsocketConnected()");
                await (Options.OnWebsocketConnected?.Invoke(_client) ?? Task.CompletedTask);
                Debug.WriteLine($"invoking Options.OnWebsocketConnected() on websocket {_clientWebSocket.GetHashCode()}");
                _connectionAttempt = 1;

                // create receiving observable
                _incomingMessages = Observable
                    .Defer(() => GetReceiveTask().ToObservable())
                    .Repeat()
                    // complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
                    .Catch<WebsocketMessageWrapper, OperationCanceledException>(exception => Observable.Empty<WebsocketMessageWrapper>())
                    .Publish();

                // subscribe maintenance
                var maintenanceSubscription = _incomingMessages.Subscribe(_ => { }, ex =>
                {
                    Debug.WriteLine($"incoming message stream {_incomingMessages.GetHashCode()} received an error: {ex}");
                    _exceptionSubject.OnNext(ex);
                    _incomingMessagesConnection?.Dispose();
                    _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
                },
                    () =>
                    {
                        Debug.WriteLine($"incoming message stream {_incomingMessages.GetHashCode()} completed");
                        _incomingMessagesConnection?.Dispose();
                        _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
                    });


                // connect observable
                var connection = _incomingMessages.Connect();
                Debug.WriteLine($"new incoming message stream {_incomingMessages.GetHashCode()} created");

                _incomingMessagesConnection = new CompositeDisposable(maintenanceSubscription, connection);
                
                var initRequest = new GraphQLWebSocketRequest
                {
                    Type = GraphQLWebSocketMessageType.GQL_CONNECTION_INIT,
                    Payload = Options.ConfigureWebSocketConnectionInitPayload(Options)
                };

                // setup task to await connection_ack message
                var ackTask = _incomingMessages
                    .Where(response => response != null )
                    .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ACK ||
                                           response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ERROR)
                    .LastAsync()
                    .ToTask();

                // send connection init
                Debug.WriteLine($"sending connection init message");
                await SendWebSocketMessageAsync(initRequest);
                var response = await ackTask;

                if (response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ACK)
                    Debug.WriteLine($"connection acknowledged: {Encoding.UTF8.GetString(response.MessageBytes)}");
                else
                {
                    var errorPayload = Encoding.UTF8.GetString(response.MessageBytes);
                    Debug.WriteLine($"connection error received: {errorPayload}");
                    throw new GraphQLWebsocketConnectionException(errorPayload);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"failed to establish websocket connection");
                _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
                _exceptionSubject.OnNext(e);
                throw;
            }
        }

        /// <summary>
        /// delay the next connection attempt using <see cref="GraphQLHttpClientOptions.BackOffStrategy"/>
        /// </summary>
        /// <returns></returns>
        private Task BackOff()
        {
            _connectionAttempt++;

            if (_connectionAttempt == 1)
                return Task.CompletedTask;

            var delay = Options.BackOffStrategy?.Invoke(_connectionAttempt - 1) ?? TimeSpan.FromSeconds(5);
            Debug.WriteLine($"connection attempt #{_connectionAttempt}, backing off for {delay.TotalSeconds} s");
            return Task.Delay(delay, _internalCancellationToken);
        }

        private IObservable<WebsocketMessageWrapper> GetMessageStream() =>
            Observable.Create<WebsocketMessageWrapper>(async observer =>
                {
                    // make sure the websocket is connected
                    await InitializeWebSocket();
                    // subscribe observer to message stream
                    var subscription = new CompositeDisposable(_incomingMessages
                        .Subscribe(observer))
                    {
                        // register the observer's OnCompleted method with the cancellation token to complete the sequence on disposal
                        _internalCancellationTokenSource.Token.Register(observer.OnCompleted)
                    };

                    // add some debug output
                    var hashCode = subscription.GetHashCode();
                    subscription.Add(Disposable.Create(() => Debug.WriteLine($"incoming message subscription {hashCode} disposed")));
                    Debug.WriteLine($"new incoming message subscription {hashCode} created");

                    return subscription;
                });

        private Task<WebsocketMessageWrapper> _receiveAsyncTask = null;
        private readonly object _receiveTaskLocker = new object();
        /// <summary>
        /// wrapper method to pick up the existing request task if already running
        /// </summary>
        /// <returns></returns>
        private Task<WebsocketMessageWrapper> GetReceiveTask()
        {
            lock (_receiveTaskLocker)
            {
                _internalCancellationToken.ThrowIfCancellationRequested();
                if (_receiveAsyncTask == null ||
                    _receiveAsyncTask.IsFaulted ||
                    _receiveAsyncTask.IsCompleted)
                    _receiveAsyncTask = ReceiveWebsocketMessagesAsync();
            }

            return _receiveAsyncTask;
        }

        /// <summary>
        /// read a single message from the websocket
        /// </summary>
        /// <returns></returns>
        private async Task<WebsocketMessageWrapper> ReceiveWebsocketMessagesAsync()
        {
            _internalCancellationToken.ThrowIfCancellationRequested();

            try
            {
                Debug.WriteLine($"waiting for data on websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})...");

                using var ms = new MemoryStream();
                WebSocketReceiveResult webSocketReceiveResult = null;
                do
                {
                    // cancellation is done implicitly via the close method
                    webSocketReceiveResult = await _clientWebSocket.ReceiveAsync(_buffer, CancellationToken.None);
                    ms.Write(_buffer.Array, _buffer.Offset, webSocketReceiveResult.Count);
                }
                while (!webSocketReceiveResult.EndOfMessage && !_internalCancellationToken.IsCancellationRequested);

                _internalCancellationToken.ThrowIfCancellationRequested();
                ms.Seek(0, SeekOrigin.Begin);

                switch (webSocketReceiveResult.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var response = await _client.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms);
                        response.MessageBytes = ms.ToArray();
                        Debug.WriteLine($"{response.MessageBytes.Length} bytes received for id {response.Id} on websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})...");
                        return response;

                    case WebSocketMessageType.Close:
                        var closeResponse = await _client.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms);
                        closeResponse.MessageBytes = ms.ToArray();
                        Debug.WriteLine($"Connection closed by the server.");
                        throw new Exception("Connection closed by the server.");

                    default:
                        throw new NotSupportedException($"Websocket message type {webSocketReceiveResult.MessageType} not supported.");

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"exception thrown while receiving websocket data: {e}");
                throw;
            }
        }

        private async Task CloseAsync()
        {
            if (_clientWebSocket == null)
                return;

            // don't attempt to close the websocket if it is in a failed state
            if (_clientWebSocket.State != WebSocketState.Open &&
                _clientWebSocket.State != WebSocketState.CloseReceived &&
                _clientWebSocket.State != WebSocketState.CloseSent)
            {
                Debug.WriteLine($"websocket {_clientWebSocket.GetHashCode()} state = {_clientWebSocket.State}");
                return;
            }

            Debug.WriteLine($"send \"connection_terminate\" message");
            await SendWebSocketMessageAsync(new GraphQLWebSocketRequest{Type = GraphQLWebSocketMessageType.GQL_CONNECTION_TERMINATE});

            Debug.WriteLine($"closing websocket {_clientWebSocket.GetHashCode()}");
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
        }

        #region IDisposable

        public void Dispose() => Complete();

        /// <summary>
        /// Cancels the current operation, closes the websocket connection and disposes of internal resources.
        /// </summary>
        public void Complete()
        {
            lock (_completedLocker)
            {
                if (Completion == null)
                    Completion = CompleteAsync();
            }
        }

        /// <summary>
        /// Task to await the completion (a.k.a. disposal) of this websocket.
        /// </summary>
        /// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
        public Task? Completion { get; private set; }

        private readonly object _completedLocker = new object();
        private async Task CompleteAsync()
        {
            Debug.WriteLine("disposing GraphQLHttpWebSocket...");

            _incomingMessagesConnection?.Dispose();

            if (!_internalCancellationTokenSource.IsCancellationRequested)
                _internalCancellationTokenSource.Cancel();

            await CloseAsync();
            _requestSubscription?.Dispose();
            _clientWebSocket?.Dispose();

            _stateSubject?.OnCompleted();
            _stateSubject?.Dispose();

            _exceptionSubject?.OnCompleted();
            _exceptionSubject?.Dispose();
            _internalCancellationTokenSource.Dispose();

            Debug.WriteLine("GraphQLHttpWebSocket disposed");
        }

        #endregion
    }
}
