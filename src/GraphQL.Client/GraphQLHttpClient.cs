using System.Diagnostics;
#pragma warning disable IDE0005
// see https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/8.0/implicit-global-using-netfx
using System.Net.Http;
#pragma warning restore IDE0005
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

    /// <summary>
    /// This flag is set to <see langword="true"/> when an error has occurred on an APQ and <see cref="GraphQLHttpClientOptions.DisableAPQ"/>
    /// has returned <see langword="true"/>. To reset this, the instance of <see cref="GraphQLHttpClient"/> has to be disposed and a new one must be created.
    /// </summary>
    public bool APQDisabledForSession { get; private set; }

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

    public GraphQLHttpClient(Action<GraphQLHttpClientOptions> configure, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient)
        : this(configure.New(), serializer, httpClient) { }

    public GraphQLHttpClient(Uri endPoint, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient)
        : this(o => o.EndPoint = endPoint, serializer, httpClient) { }

    public GraphQLHttpClient(string endPoint, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient)
        : this(new Uri(endPoint), serializer, httpClient) { }

    #endregion

    #region IGraphQLClient

    private const int APQ_SUPPORTED_VERSION = 1;

    /// <inheritdoc />
    public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? savedQuery = request.Query;
        bool useAPQ = false;

        if (request.Query != null && !APQDisabledForSession && Options.EnableAutomaticPersistedQueries(request))
        {
            // https://www.apollographql.com/docs/react/api/link/persisted-queries/
            useAPQ = true;
            request.Extensions ??= new();
            request.Extensions["persistedQuery"] = new Dictionary<string, object>
            {
                ["version"] = APQ_SUPPORTED_VERSION,
                ["sha256Hash"] = Hash.Compute(request.Query),
            };
            request.Query = null;
        }

        var response = await SendQueryInternalAsync<TResponse>(request, cancellationToken);

        if (useAPQ)
        {
            if (response.Errors?.Any(error => string.Equals(error.Message, "PersistedQueryNotFound", StringComparison.CurrentCultureIgnoreCase)) == true)
            {
                // GraphQL server supports APQ!

                // Alas, for the first time we did not guess and in vain removed Query, so we return Query and
                // send request again. This is one-time "cache miss", not so scary.
                request.Query = savedQuery;
                return await SendQueryInternalAsync<TResponse>(request, cancellationToken);
            }
            else
            {
                // GraphQL server either supports APQ of some other version, or does not support it at all.
                // Send a request for the second time. This is better than returning an error. Let the client work with APQ disabled.
                APQDisabledForSession = Options.DisableAPQ(response);
                request.Query = savedQuery;
                return await SendQueryInternalAsync<TResponse>(request, cancellationToken);
            }
        }

        return response;
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
    private async Task<GraphQLResponse<TResponse>> SendQueryInternalAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) =>
        Options.UseWebSocketForQueriesAndMutations || Options.WebSocketEndPoint is not null && Options.EndPoint is null || Options.EndPoint.HasWebSocketScheme()
            ? await GraphQlHttpWebSocket.SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false)
            : await SendHttpRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);

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
