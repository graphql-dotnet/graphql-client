using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;


internal class GraphQLHttpWebSocket : IDisposable
{

    #region Private fields

    protected readonly Uri _webSocketUri;
    protected readonly GraphQLHttpClient _client;
    private readonly ArraySegment<byte> _buffer;
    private readonly CancellationTokenSource _internalCancellationTokenSource = new();
    protected readonly CancellationToken _internalCancellationToken;
    private readonly Subject<GraphQLWebSocketRequest> _requestSubject = new();
    protected readonly Subject<Exception> _exceptionSubject = new();
    protected readonly BehaviorSubject<GraphQLWebsocketConnectionState> _stateSubject = new(GraphQLWebsocketConnectionState.Disconnected);
    private readonly IDisposable _requestSubscription;

    protected int _connectionAttempt = 0;
    protected IConnectableObservable<WebsocketMessageWrapper> _incomingMessages;
    protected IDisposable _incomingMessagesConnection;
    private IWebsocketProtocolHandler? _websocketProtocolHandler;
    protected GraphQLHttpClientOptions Options => _client.Options;

    private Task _initializeWebSocketTask = Task.CompletedTask;
    private readonly object _initializeLock = new();


#if NETFRAMEWORK
    protected WebSocket? _clientWebSocket = null;
#else
    protected ClientWebSocket? _clientWebSocket = null;
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

    /// <summary>
    /// The websocket protocol used for subscriptions or full-websocket connections
    /// </summary>
    public string? WebsocketProtocol => _websocketProtocolHandler?.WebsocketProtocol;


    public IObservable<object?> Pongs => null;

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

    /// <summary>
    /// Returns the pong message stream. Subscribing initiates the websocket connection if not already established.
    /// </summary>
    /// <returns></returns>
    public IObservable<object?> GetPongStream() =>
        Observable.Defer(async () =>
            {
                if (_websocketProtocolHandler is null)
                {
                    await InitializeWebSocket().ConfigureAwait(false);
                }

                return _websocketProtocolHandler.CreatePongObservable();
            })
            // complete sequence on OperationCanceledException, this is triggered by the cancellation token
            .Catch<object?, OperationCanceledException>(exception =>
                Observable.Empty<object?>());

    #region Send requests

    /// <inheritdoc cref="IGraphQLWebSocketClient.SendPingAsync"/>
    public async Task SendPingAsync(object? payload)
    {
        if (_websocketProtocolHandler is null)
        {
            await InitializeWebSocket().ConfigureAwait(false);
        }

        await _websocketProtocolHandler.SendPingAsync(payload);
    }

    /// <inheritdoc cref="IGraphQLWebSocketClient.SendPongAsync"/>
    public async Task SendPongAsync(object? payload)
    {
        if (_websocketProtocolHandler is null)
        {
            await InitializeWebSocket().ConfigureAwait(false);
        }

        await _websocketProtocolHandler.SendPongAsync(payload);
    }

    /// <summary>
    /// Send a regular GraphQL request (query, mutation) via websocket
    /// </summary>
    /// <typeparam name="TResponse">the response type</typeparam>
    /// <param name="request">the <see cref="GraphQLRequest"/> to send</param>
    /// <param name="cancellationToken">the token to cancel the request</param>
    /// <returns></returns>
    public async Task<GraphQLResponse<TResponse>> SendRequestAsync<TResponse>(GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_websocketProtocolHandler is null)
        {
            await InitializeWebSocket().ConfigureAwait(false);
        }

        return await _websocketProtocolHandler.CreateGraphQLRequestObservable<TResponse>(request)
            // complete sequence on OperationCanceledException, this is triggered by the cancellation token
            .Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
                Observable.Empty<GraphQLResponse<TResponse>>())
            .FirstAsync()
            .ToTask(cancellationToken);
    }

    /// <summary>
    /// Create a new subscription stream
    /// </summary>
    /// <typeparam name="TResponse">the response type</typeparam>
    /// <param name="request">the <see cref="GraphQLRequest"/> to start the subscription</param>
    /// <param name="exceptionHandler">Optional: exception handler for handling exceptions within the receive pipeline</param>
    /// <returns>a <see cref="IObservable{TResponse}"/> which represents the subscription</returns>
    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception>? exceptionHandler = null) =>
        Observable.Defer(async () =>
            {
                if (_websocketProtocolHandler is null)
                {
                    await InitializeWebSocket().ConfigureAwait(false);
                }
                return _websocketProtocolHandler?.CreateSubscriptionObservable<TResponse>(request);
            })
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
            });

    protected Task QueueWebSocketRequest(GraphQLWebSocketRequest request)
    {
        _requestSubject.OnNext(request);
        return request.SendTask();
    }

    protected async Task<Unit> SendWebSocketRequestAsync(GraphQLWebSocketRequest request)
    {
        try
        {
            if (_internalCancellationToken.IsCancellationRequested)
            {
                request.SendCanceled();
                return Unit.Default;
            }

            await InitializeWebSocket().ConfigureAwait(false);
            await SendWebSocketMessageAsync(request, _internalCancellationToken).ConfigureAwait(false);
            request.SendCompleted();
        }
        catch (Exception e)
        {
            request.SendFailed(e);
        }
        return Unit.Default;
    }

    protected async Task SendWebSocketMessageAsync(GraphQLWebSocketRequest request, CancellationToken cancellationToken = default)
    {
        var requestBytes = _client.JsonSerializer.SerializeToBytes(request);
        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>(requestBytes),
            WebSocketMessageType.Text,
            true,
            cancellationToken).ConfigureAwait(false);
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
            switch (_clientWebSocket)
            {
                case ClientWebSocket nativeWebSocket:
                    ConfigureWebSocketSubProtocols(nativeWebSocket.Options.AddSubProtocol);
                    nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                    nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                    Options.ConfigureWebsocketOptions(nativeWebSocket.Options);
                    break;
                case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
                    ConfigureWebSocketSubProtocols(managedWebSocket.Options.AddSubProtocol);
                    managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                    managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                    break;
                default:
                    throw new NotSupportedException($"unknown websocket type {_clientWebSocket.GetType().Name}");
            }
#else
            _clientWebSocket = new ClientWebSocket();
            ConfigureWebSocketSubProtocols(_clientWebSocket.Options.AddSubProtocol);

            // the following properties are not supported in Blazor WebAssembly and throw a PlatformNotSupportedException error when accessed
            try
            {
                var certs = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                if (certs != null) // ClientWebSocketOptions.ClientCertificates.set throws ArgumentNullException
                    _clientWebSocket.Options.ClientCertificates = certs;
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

    public void ConfigureWebSocketSubProtocols(Action<string> addSubProtocol)
    {
        if (_client.Options.WebSocketProtocol is null)
        {
            foreach (string protocol in WebSocketProtocols.GetSupportedWebSocketProtocols())
            {
                addSubProtocol(protocol);
            }
        }
        else
            addSubProtocol(_client.Options.WebSocketProtocol);
    }


    protected async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            //Client sends a WebSocket handshake request with the sub-protocol: graphql-transport-ws
            await BackOff().ConfigureAwait(false);
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connecting);
            Debug.WriteLine($"opening websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})");
            await _clientWebSocket.ConnectAsync(_webSocketUri, token).ConfigureAwait(false);
            // check negotiated sub protocol and create matching instance of IWebsocketProtocolHandler
            _websocketProtocolHandler = _clientWebSocket.SubProtocol switch
            {
                WebSocketProtocols.GRAPHQL_WS
                    => new GraphQLWSProtocolHandler(this, _client, QueueWebSocketRequest, SendWebSocketMessageAsync),
                WebSocketProtocols.GRAPHQL_TRANSPORT_WS
                    => new GraphQLTransportWSProtocolHandler(this, _client, QueueWebSocketRequest, SendWebSocketMessageAsync),
                _ => throw new NotSupportedException(
                    $"negotiated websocket protocol \"{_clientWebSocket.SubProtocol}\" not supported")
            };
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connected);
            Debug.WriteLine($"connection established on websocket {_clientWebSocket.GetHashCode()}, invoking Options.OnWebsocketConnected()");
            await (Options.OnWebsocketConnected?.Invoke(_client) ?? Task.CompletedTask).ConfigureAwait(false);
            Debug.WriteLine($"invoking Options.OnWebsocketConnected() on websocket {_clientWebSocket.GetHashCode()}");
            _connectionAttempt = 1;

            // Client immediately dispatches a ConnectionInit message optionally providing a payload as agreed with the server

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

            var closeConnectionDisposable = new CompositeDisposable();
            _incomingMessagesConnection = new CompositeDisposable(connection, maintenanceSubscription, closeConnectionDisposable);

            await _websocketProtocolHandler.InitializeConnectionAsync(_incomingMessages, closeConnectionDisposable).ConfigureAwait(false);
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
    protected Task BackOff()
    {
        _connectionAttempt++;

        if (_connectionAttempt == 1)
            return Task.CompletedTask;

        var delay = Options.BackOffStrategy?.Invoke(_connectionAttempt - 1) ?? TimeSpan.FromSeconds(5);
        Debug.WriteLine($"connection attempt #{_connectionAttempt}, backing off for {delay.TotalSeconds} s");
        return Task.Delay(delay, _internalCancellationToken);
    }

    protected IObservable<WebsocketMessageWrapper> GetMessageStream() =>
        Observable.Create<WebsocketMessageWrapper>(async observer =>
            {
                // make sure the websocket is connected
                await InitializeWebSocket().ConfigureAwait(false);
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

    protected Task<WebsocketMessageWrapper> _receiveAsyncTask = null;
    protected readonly object _receiveTaskLocker = new();
    /// <summary>
    /// wrapper method to pick up the existing request task if already running
    /// </summary>
    /// <returns></returns>
    protected Task<WebsocketMessageWrapper> GetReceiveTask()
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
    protected async Task<WebsocketMessageWrapper> ReceiveWebsocketMessagesAsync()
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
                webSocketReceiveResult = await _clientWebSocket.ReceiveAsync(_buffer, CancellationToken.None).ConfigureAwait(false);
                ms.Write(_buffer.Array, _buffer.Offset, webSocketReceiveResult.Count);
            }
            while (!webSocketReceiveResult.EndOfMessage && !_internalCancellationToken.IsCancellationRequested);

            _internalCancellationToken.ThrowIfCancellationRequested();
            ms.Seek(0, SeekOrigin.Begin);

            switch (webSocketReceiveResult.MessageType)
            {
                case WebSocketMessageType.Text:
                    var response = await _client.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms).ConfigureAwait(false);
                    response.MessageBytes = ms.ToArray();
                    Debug.WriteLine($"{response.MessageBytes.Length} bytes received for id {response.Id} on websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})...");
                    return response;

                case WebSocketMessageType.Close:
                    WebsocketMessageWrapper closeResponse = null;
                    try
                    {
                        closeResponse = await _client.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    if (closeResponse != null)
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

    protected async Task CloseAsync()
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

        if (_websocketProtocolHandler is not null)
        {
            Debug.WriteLine($"send \"connection_terminate\" message");
            await _websocketProtocolHandler.SendCloseConnectionRequestAsync().ConfigureAwait(false);
        }

        Debug.WriteLine($"closing websocket {_clientWebSocket.GetHashCode()}");
        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).ConfigureAwait(false);
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
            Completion ??= CompleteAsync();
        }
    }

    /// <summary>
    /// Task to await the completion (a.k.a. disposal) of this websocket.
    /// </summary>
    /// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
    public Task? Completion { get; protected set; }

    protected readonly object _completedLocker = new();
    protected async Task CompleteAsync()
    {
        Debug.WriteLine("disposing GraphQLHttpWebSocket...");

        _incomingMessagesConnection?.Dispose();

        if (!_internalCancellationTokenSource.IsCancellationRequested)
            _internalCancellationTokenSource.Cancel();

        await CloseAsync().ConfigureAwait(false);
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
