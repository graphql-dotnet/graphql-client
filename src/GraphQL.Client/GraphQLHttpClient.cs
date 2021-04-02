using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;

namespace GraphQL.Client.Http
{
    public class GraphQLHttpClient : IGraphQLClient
    {
        private readonly Lazy<GraphQLHttpWebSocket> _lazyHttpWebSocket;
        private GraphQLHttpWebSocket GraphQlHttpWebSocket => _lazyHttpWebSocket.Value;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object> _subscriptionStreams = new ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object>();

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
        /// Publishes all exceptions which occur inside the websocket receive stream (i.e. for logging purposes)
        /// </summary>
        public IObservable<Exception> WebSocketReceiveErrors => GraphQlHttpWebSocket.ReceiveErrors;

        /// <summary>
        /// the websocket connection state
        /// </summary>
        public IObservable<GraphQLWebsocketConnectionState> WebsocketConnectionState => GraphQlHttpWebSocket.ConnectionState;

        #region Constructors

        public GraphQLHttpClient(string endPoint, IGraphQLWebsocketJsonSerializer serializer) : this(new Uri(endPoint), serializer) { }

        public GraphQLHttpClient(Uri endPoint, IGraphQLWebsocketJsonSerializer serializer) : this(o => o.EndPoint = endPoint, serializer) { }

        public GraphQLHttpClient(Action<GraphQLHttpClientOptions> configure, IGraphQLWebsocketJsonSerializer serializer) : this(configure.New(), serializer) { }

        public GraphQLHttpClient(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer) : this(
            options, serializer, new HttpClient(options.HttpMessageHandler))
        {
            // set this flag to dispose the internally created HttpClient when GraphQLHttpClient gets disposed
            _disposeHttpClient = true;
        }

        public GraphQLHttpClient(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            JsonSerializer = serializer ?? throw new ArgumentNullException(nameof(serializer), "please configure the JSON serializer you want to use");
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (!HttpClient.DefaultRequestHeaders.UserAgent.Any())
                HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(GetType().Assembly.GetName().Name, GetType().Assembly.GetName().Version.ToString()));
            
            _lazyHttpWebSocket = new Lazy<GraphQLHttpWebSocket>(CreateGraphQLHttpWebSocket);
        }

        #endregion

        #region IGraphQLClient

        /// <inheritdoc />
        public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            if (Options.UseWebSocketForQueriesAndMutations ||
                !(Options.WebSocketEndPoint is null) && Options.EndPoint is null ||
                Options.EndPoint.HasWebSocketScheme())
                return await GraphQlHttpWebSocket.SendRequest<TResponse>(request, cancellationToken);

            return await SendHttpRequestAsync<TResponse>(request, cancellationToken);
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

            var observable = GraphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request);

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

            var observable = GraphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request, exceptionHandler);
            _subscriptionStreams.TryAdd(key, observable);
            return observable;
        }

        #endregion

        /// <summary>
        /// explicitly opens the websocket connection. Will be closed again on disposing the last subscription
        /// </summary>
        /// <returns></returns>
        public Task InitializeWebsocketConnection() => GraphQlHttpWebSocket.InitializeWebSocket();

        #region Private Methods

        private async Task<GraphQLHttpResponse<TResponse>> SendHttpRequestAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            var preprocessedRequest = await Options.PreprocessRequest(request, this);

            using var httpRequestMessage = preprocessedRequest.ToHttpRequestMessage(Options, JsonSerializer);
            using var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var graphQLResponse = await JsonSerializer.DeserializeFromUtf8StreamAsync<TResponse>(contentStream, cancellationToken);
                return graphQLResponse.ToGraphQLHttpResponse(httpResponseMessage.Headers, httpResponseMessage.StatusCode);
            }

            // error handling
            string content = null;
            if (contentStream != null)
                using (var sr = new StreamReader(contentStream))
                    content = await sr.ReadToEndAsync();

            throw new GraphQLHttpRequestException(httpResponseMessage.StatusCode, httpResponseMessage.Headers, content);
        }

        private GraphQLHttpWebSocket CreateGraphQLHttpWebSocket()
        {
            if(Options.WebSocketEndPoint is null && Options.EndPoint is null)
                throw new InvalidOperationException("no endpoint configured");

            var webSocketEndpoint = Options.WebSocketEndPoint ?? Options.EndPoint.GetWebSocketUri();
            if (!webSocketEndpoint.HasWebSocketScheme())
                throw new InvalidOperationException($"uri \"{webSocketEndpoint}\" is not a websocket endpoint");

            return new GraphQLHttpWebSocket(webSocketEndpoint, this);
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
        private readonly object _disposeLocker = new object();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Debug.WriteLine($"Disposing GraphQLHttpClient on endpoint {Options.EndPoint}");
                _cancellationTokenSource.Cancel();
                if(_disposeHttpClient)
                    HttpClient.Dispose();
                if ( _lazyHttpWebSocket.IsValueCreated )
                    _lazyHttpWebSocket.Value.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        #endregion
    }
}
