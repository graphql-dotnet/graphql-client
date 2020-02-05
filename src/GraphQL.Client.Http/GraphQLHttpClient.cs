using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http.Websocket;

namespace GraphQL.Client.Http {

	public class GraphQLHttpClient : IGraphQLClient {

		private readonly GraphQLHttpWebSocket graphQlHttpWebSocket;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly HttpClient httpClient;
		private readonly ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object> subscriptionStreams = new ConcurrentDictionary<Tuple<GraphQLRequest, Type>, object>();

		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLHttpClientOptions Options { get; }

		/// <inheritdoc />
		public IObservable<Exception> WebSocketReceiveErrors => graphQlHttpWebSocket.ReceiveErrors;

		public GraphQLHttpClient(string endPoint) : this(new Uri(endPoint)) { }

		public GraphQLHttpClient(Uri endPoint) : this(o => o.EndPoint = endPoint) { }

		public GraphQLHttpClient(Action<GraphQLHttpClientOptions> configure) {
			Options = new GraphQLHttpClientOptions();
			configure(Options);
			this.httpClient = new HttpClient();
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(GetWebSocketUri(), Options);
		}

		public GraphQLHttpClient(GraphQLHttpClientOptions options) {
			Options = options;
			this.httpClient = new HttpClient();
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(GetWebSocketUri(), Options);
		}

		public GraphQLHttpClient(GraphQLHttpClientOptions options, HttpClient httpClient) {
			Options = options;
			this.httpClient = httpClient;
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(GetWebSocketUri(), Options);
		}

		public Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) {
			return Options.UseWebSocketForQueriesAndMutations
				? this.graphQlHttpWebSocket.Request<TResponse>(request, Options, cancellationToken)
				: this.SendHttpPostRequestAsync<TResponse>(request, cancellationToken);
		}

		public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) {
			return Options.UseWebSocketForQueriesAndMutations
				? this.graphQlHttpWebSocket.Request<TResponse>(request, Options, cancellationToken)
				: this.SendHttpPostRequestAsync<TResponse>(request, cancellationToken);
		}

		/// <inheritdoc />
		public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request) {
			if (disposed)
				throw new ObjectDisposedException(nameof(GraphQLHttpClient));

			var key = new Tuple<GraphQLRequest, Type>(request, typeof(TResponse));

			if (subscriptionStreams.ContainsKey(key))
				return (IObservable<GraphQLResponse<TResponse>>)subscriptionStreams[key];

			var observable = graphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request, Options, cancellationToken: cancellationTokenSource.Token);

			subscriptionStreams.TryAdd(key, observable);
			return observable;
		}
		
		/// <inheritdoc />
		public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler) {
			if (disposed)
				throw new ObjectDisposedException(nameof(GraphQLHttpClient));

			var key = new Tuple<GraphQLRequest, Type>(request, typeof(TResponse));

			if (subscriptionStreams.ContainsKey(key))
				return (IObservable<GraphQLResponse<TResponse>>)subscriptionStreams[key];

			var observable = graphQlHttpWebSocket.CreateSubscriptionStream<TResponse>(request, Options, exceptionHandler, cancellationTokenSource.Token);
			subscriptionStreams.TryAdd(key, observable);
			return observable;
		}

		/// <summary>
		/// explicitly opens the websocket connection. Will be closed again on disposing the last subscription
		/// </summary>
		/// <returns></returns>
		public Task InitializeWebsocketConnection() => graphQlHttpWebSocket.InitializeWebSocket();

		#region Private Methods

		private async Task<GraphQLResponse<TResponse>> SendHttpPostRequestAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) {
			using var httpRequestMessage = this.GenerateHttpRequestMessage(request.SerializeToJson(Options));
			using var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);
			if (!httpResponseMessage.IsSuccessStatusCode) {
				throw new GraphQLHttpException(httpResponseMessage);
			}

			var bodyStream = await httpResponseMessage.Content.ReadAsStreamAsync();
			return await bodyStream.DeserializeFromJsonAsync<GraphQLHttpResponse<TResponse>>(Options, cancellationToken);
		}

		private HttpRequestMessage GenerateHttpRequestMessage(string requestString) {
			return new HttpRequestMessage(HttpMethod.Post, this.Options.EndPoint) {
				Content = new StringContent(requestString, Encoding.UTF8, "application/json")
			};
		}

		private Uri GetWebSocketUri() {
			var webSocketSchema = this.Options.EndPoint.Scheme == "https" ? "wss" : "ws";
			return new Uri($"{webSocketSchema}://{this.Options.EndPoint.Host}:{this.Options.EndPoint.Port}{this.Options.EndPoint.AbsolutePath}");
		}

		#endregion


		#region IDisposable

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose() {
			lock (disposeLocker) {
				if (!disposed) {
					_dispose();
				}
			}
		}

		private bool disposed = false;
		private readonly object disposeLocker = new object();

		private void _dispose() {
			disposed = true;
			this.httpClient.Dispose();
			this.graphQlHttpWebSocket.Dispose();
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
		}

		#endregion

	}

}
