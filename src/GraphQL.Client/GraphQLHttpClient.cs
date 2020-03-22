using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Client.Http
{

    public class GraphQLHttpClient : IGraphQLClient
    {

        private readonly GraphQLHttpWebSocket _graphQlHttpWebSocket;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object> _subscriptionStreams = new ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object>();
        private IGraphQLWebsocketJsonSerializer JsonSerializer => Options.JsonSerializer;

        /// <summary>
        /// the instance of <see cref="HttpClient"/> which is used internally
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The Options	to be used
        /// </summary>
        public GraphQLHttpClientOptions Options { get; }

        /// <summary>
        /// Publishes all exceptions which occur inside the websocket receive stream (i.e. for logging purposes)
        /// </summary>
        public IObservable<Exception> WebSocketReceiveErrors => _graphQlHttpWebSocket.ReceiveErrors;

        /// <summary>
        /// the websocket connection state
        /// </summary>
        public IObservable<GraphQLWebsocketConnectionState> WebsocketConnectionState =>
            _graphQlHttpWebSocket.ConnectionState;

        #region Constructors

        public GraphQLHttpClient(string endPoint) : this(new Uri(endPoint)) { }

        public GraphQLHttpClient(Uri endPoint) : this(o => o.EndPoint = endPoint) { }

        public GraphQLHttpClient(Action<GraphQLHttpClientOptions> configure) : this(configure.New()) { }

        public GraphQLHttpClient(GraphQLHttpClientOptions options) : this(options, new HttpClient(options.HttpMessageHandler)) { }

        public GraphQLHttpClient(GraphQLHttpClientOptions options, HttpClient httpClient) : this(options, httpClient, new NewtonsoftJsonSerializer()) { }

        public GraphQLHttpClient(GraphQLHttpClientOptions options, HttpClient httpClient, IGraphQLWebsocketJsonSerializer serializer)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Options.JsonSerializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _graphQlHttpWebSocket = new GraphQLHttpWebSocket(GetWebSocketUri(), this);
        }

        #endregion

        #region IGraphQLClient

        /// <inheritdoc />
        public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            if (Options.UseWebSocketForQueriesAndMutations)
                return await _graphQlHttpWebSocket.SendRequest<TResponse>(request, cancellationToken);

            return await SendHttpPostRequestAsync<TResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request,
            CancellationToken cancellationToken = default)
            => SendQueryAsync<TResponse>(request, cancellationToken);

        /// <inheritdoc />
        public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GraphQLHttpClient));

            var key = new Tuple<GraphQLRequest, Type>(request, typeof(TResponse));

            if (_subscriptionStreams.ContainsKey(key))
                return (IObservable<GraphQLResponse<TResponse>>)_subscriptionStreams[key];

            var observable = _graphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request);

            _subscriptionStreams.TryAdd(key, observable);
            return observable;
        }

        /// <inheritdoc />
        public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GraphQLHttpClient));

            var key = new Tuple<GraphQLRequest, Type>(request, typeof(TResponse));

            if (_subscriptionStreams.ContainsKey(key))
                return (IObservable<GraphQLResponse<TResponse>>)_subscriptionStreams[key];

            var observable = _graphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request, exceptionHandler);
            _subscriptionStreams.TryAdd(key, observable);
            return observable;
        }

        #endregion

        /// <summary>
        /// explicitly opens the websocket connection. Will be closed again on disposing the last subscription
        /// </summary>
        /// <returns></returns>
        public Task InitializeWebsocketConnection() => _graphQlHttpWebSocket.InitializeWebSocket();

        #region Private Methods

        private async Task<GraphQLHttpResponse<TResponse>> SendHttpPostRequestAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            var preprocessedRequest = await Options.PreprocessRequest(request, this);
            using var httpRequestMessage = GenerateHttpRequestMessage(preprocessedRequest);
            using var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, cancellationToken);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new GraphQLHttpException(httpResponseMessage);
            }

            var bodyStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeFromUtf8StreamAsync<TResponse>(bodyStream, cancellationToken);
            return response.ToGraphQLHttpResponse(httpResponseMessage.Headers, httpResponseMessage.StatusCode);
        }

        private HttpRequestMessage GenerateHttpRequestMessage(GraphQLRequest request)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, Options.EndPoint)
            {
                Content = new StringContent(JsonSerializer.SerializeToString(request), Encoding.UTF8, Options.MediaType)
            };

            foreach(var header in Options.Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }

            if (request is GraphQLHttpRequest httpRequest)
                httpRequest.PreprocessHttpRequestMessage(message);

            return message;
        }

        private Uri GetWebSocketUri()
        {
            var webSocketSchema = Options.EndPoint.Scheme == "https" ? "wss" : "ws";
            return new Uri($"{webSocketSchema}://{Options.EndPoint.Host}:{Options.EndPoint.Port}{Options.EndPoint.AbsolutePath}");
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
                    _dispose();
                }
            }
        }

        private bool _disposed = false;
        private readonly object _disposeLocker = new object();

        private void _dispose()
        {
            _disposed = true;
            Debug.WriteLine($"disposing GraphQLHttpClient on endpoint {Options.EndPoint}");
            _cancellationTokenSource.Cancel();
            HttpClient.Dispose();
            _graphQlHttpWebSocket.Dispose();
            _cancellationTokenSource.Dispose();
        }

        #endregion

    }

}
