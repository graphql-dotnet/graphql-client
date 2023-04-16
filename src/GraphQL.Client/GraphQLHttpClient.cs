using System.Diagnostics;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;

namespace GraphQL.Client.Http;

public class GraphQLHttpClient : IGraphQLWebSocketClient, IDisposable
{
    private readonly Lazy<GraphQLHttpWebSocket> _lazyHttpWebSocket;
    private GraphQLHttpWebSocket GraphQlHttpWebSocket => _lazyHttpWebSocket.Value;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly bool _disposeHttpClient = false;

    /// <summary>
    /// the json serializer
    /// </summary>
    public IGraphQLWebsocketJsonSerializer JsonSerializer { get; }

    /// <summary>
    /// the instance of <see cref="HttpClient"/> which is used internally
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    /// The Options	to be used
    /// </summary>
    public GraphQLHttpClientOptions Options { get; }

    /// <inheritdoc />
    public IObservable<Exception> WebSocketReceiveErrors => GraphQlHttpWebSocket.ReceiveErrors;

    /// <inheritdoc />
    public string? WebSocketSubProtocol => GraphQlHttpWebSocket.WebsocketProtocol;

    /// <inheritdoc />
    public IObservable<GraphQLWebsocketConnectionState> WebsocketConnectionState => GraphQlHttpWebSocket.ConnectionState;

    /// <inheritdoc />
    public IObservable<object?> PongStream => GraphQlHttpWebSocket.GetPongStream();

    #region Constructors

    public GraphQLHttpClient(string endPoint, IGraphQLWebsocketJsonSerializer serializer)
        : this(new Uri(endPoint), serializer) { }

    public GraphQLHttpClient(Uri endPoint, IGraphQLWebsocketJsonSerializer serializer)
        : this(o => o.EndPoint = endPoint, serializer) { }

    public GraphQLHttpClient(Action<GraphQLHttpClientOptions> configure, IGraphQLWebsocketJsonSerializer serializer)
        : this(configure.New(), serializer) { }

    public GraphQLHttpClient(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer)
        : this(options, serializer, new HttpClient(options.HttpMessageHandler))
    {
        // set this flag to dispose the internally created HttpClient when GraphQLHttpClient gets disposed
        _disposeHttpClient = true;
    }

    public GraphQLHttpClient(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        JsonSerializer = serializer ?? throw new ArgumentNullException(nameof(serializer), "please configure the JSON serializer you want to use");
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _lazyHttpWebSocket = new Lazy<GraphQLHttpWebSocket>(CreateGraphQLHttpWebSocket);
    }

    #endregion

    #region IGraphQLClient

    /// <inheritdoc />
    public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        return Options.UseWebSocketForQueriesAndMutations || Options.WebSocketEndPoint is not null && Options.EndPoint is null || Options.EndPoint.HasWebSocketScheme()
            ? await GraphQlHttpWebSocket.SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false)
            : await SendHttpRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request,
        CancellationToken cancellationToken = default)
        => SendQueryAsync<TResponse>(request, cancellationToken);

    /// <inheritdoc />
    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request)
        => CreateSubscriptionStream<TResponse>(request, null);

    /// <inheritdoc />
    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception>? exceptionHandler)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GraphQLHttpClient));

        var observable = GraphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request, exceptionHandler);
        return observable;
    }

    #endregion

    /// <inheritdoc />
    public Task InitializeWebsocketConnection() => GraphQlHttpWebSocket.InitializeWebSocket();

    /// <inheritdoc />
    public Task SendPingAsync(object? payload) => GraphQlHttpWebSocket.SendPingAsync(payload);

    /// <inheritdoc />
    public Task SendPongAsync(object? payload) => GraphQlHttpWebSocket.SendPongAsync(payload);

    #region Private Methods

    private async Task<GraphQLHttpResponse<TResponse>> SendHttpRequestAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        var preprocessedRequest = await Options.PreprocessRequest(request, this).ConfigureAwait(false);

        using var httpRequestMessage = preprocessedRequest.ToHttpRequestMessage(Options, JsonSerializer);
        using var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

        if (Options.IsValidResponseToDeserialize(httpResponseMessage))
        {
            var graphQLResponse = await JsonSerializer.DeserializeFromUtf8StreamAsync<TResponse>(contentStream, cancellationToken).ConfigureAwait(false);
            return graphQLResponse.ToGraphQLHttpResponse(httpResponseMessage.Headers, httpResponseMessage.StatusCode);
        }

        // error handling
        string content = null;
        if (contentStream != null)
        {
            using var sr = new StreamReader(contentStream);
            content = await sr.ReadToEndAsync().ConfigureAwait(false);
        }

        throw new GraphQLHttpRequestException(httpResponseMessage.StatusCode, httpResponseMessage.Headers, content);
    }

    private GraphQLHttpWebSocket CreateGraphQLHttpWebSocket()
    {
        if (Options.WebSocketEndPoint is null && Options.EndPoint is null)
            throw new InvalidOperationException("no endpoint configured");

        var webSocketEndpoint = Options.WebSocketEndPoint ?? Options.EndPoint.GetWebSocketUri();
        return !webSocketEndpoint.HasWebSocketScheme()
            ? throw new InvalidOperationException($"uri \"{webSocketEndpoint}\" is not a websocket endpoint")
            : new GraphQLHttpWebSocket(webSocketEndpoint, this);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases unmanaged resources
    /// </summary>
    public void Dispose()
    {
        lock (_disposeLocker)
        {
            if (!_disposed)
            {
                _disposed = true;
                Dispose(true);
            }
        }
    }

    private volatile bool _disposed;
    private readonly object _disposeLocker = new();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Debug.WriteLine($"Disposing GraphQLHttpClient on endpoint {Options.EndPoint}");
            _cancellationTokenSource.Cancel();
            if (_disposeHttpClient)
                HttpClient.Dispose();
            if (_lazyHttpWebSocket.IsValueCreated)
                _lazyHttpWebSocket.Value.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }

    #endregion
}
