using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;


internal abstract class BaseGraphQLHttpWebSocket : IDisposable
{

    #region Private fields

    protected readonly Uri _webSocketUri;
    protected readonly GraphQLHttpClient _client;
    protected readonly ArraySegment<byte> _buffer;
    protected readonly CancellationTokenSource _internalCancellationTokenSource = new CancellationTokenSource();
    protected readonly CancellationToken _internalCancellationToken;
    protected readonly Subject<GraphQLWebSocketRequest> _requestSubject = new Subject<GraphQLWebSocketRequest>();
    protected readonly Subject<Exception> _exceptionSubject = new Subject<Exception>();
    protected readonly BehaviorSubject<GraphQLWebsocketConnectionState> _stateSubject =
        new BehaviorSubject<GraphQLWebsocketConnectionState>(GraphQLWebsocketConnectionState.Disconnected);
    protected readonly IDisposable _requestSubscription;

    protected int _connectionAttempt = 0;
    protected IConnectableObservable<WebsocketMessageWrapper> _incomingMessages;
    protected IDisposable _incomingMessagesConnection;
    protected GraphQLHttpClientOptions Options => _client.Options;

    protected Task _initializeWebSocketTask = Task.CompletedTask;
    protected readonly object _initializeLock = new object();


#if NETFRAMEWORK
    protected WebSocket _clientWebSocket = null;
#else
    protected ClientWebSocket _clientWebSocket = null;
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
    /// Publishes the connection state of the <see cref="BaseGraphQLHttpWebSocket"/>
    /// </summary>
    public IObservable<GraphQLWebsocketConnectionState> ConnectionState => _stateSubject.DistinctUntilChanged();

    /// <summary>
    /// Publishes all messages which are received on the websocket
    /// </summary>
    public IObservable<WebsocketMessageWrapper> IncomingMessageStream { get; }

    /// <summary>
    /// The websocket protocol used for subscriptions or full-websocket connections
    /// </summary>
    public abstract string WebsocketProtocol { get; }
    #endregion

    public BaseGraphQLHttpWebSocket(Uri webSocketUri, GraphQLHttpClient client)
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
    public abstract IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception>? exceptionHandler = null);


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
            var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client).ConfigureAwait(false);
            var websocketRequest = new GraphQLWebSocketRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = GraphQLWebSocketMessageType.GQL_START,
                Payload = preprocessedRequest
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
                await InitializeWebSocket().ConfigureAwait(false);
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
                await QueueWebSocketRequest(websocketRequest).ConfigureAwait(false);
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
                    nativeWebSocket.Options.AddSubProtocol(WebsocketProtocol);
                    nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                    nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                    Options.ConfigureWebsocketOptions(nativeWebSocket.Options);
                    break;
                case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
                    managedWebSocket.Options.AddSubProtocol(WebsocketProtocol);
                    managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)Options.HttpMessageHandler).ClientCertificates;
                    managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)Options.HttpMessageHandler).UseDefaultCredentials;
                    break;
                default:
                    throw new NotSupportedException($"unknown websocket type {_clientWebSocket.GetType().Name}");
            }
#else
            _clientWebSocket = new ClientWebSocket();
            _clientWebSocket.Options.AddSubProtocol(WebsocketProtocol);

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

    protected abstract Task ConnectAsync(CancellationToken token);


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
    protected readonly object _receiveTaskLocker = new object();
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
                    var closeResponse = await _client.JsonSerializer.DeserializeToWebsocketResponseWrapperAsync(ms).ConfigureAwait(false);
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

    protected abstract Task CloseAsync();

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

    protected readonly object _completedLocker = new object();
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
