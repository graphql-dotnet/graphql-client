using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Http {

	public class GraphQLHttpClient : IGraphQLClient {

		private readonly GraphQLHttpWebSocket graphQlHttpWebSocket;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private readonly HttpClient httpClient;
		
		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLHttpClientOptions Options { get; }

		/// <inheritdoc />
		public IObservable<Exception> WebSocketReceiveErrors => graphQlHttpWebSocket.ReceiveErrors;


		public GraphQLHttpClient(string endPoint) : this(new Uri(endPoint))
		{ }

		public GraphQLHttpClient(Uri endPoint) : this(o => o.EndPoint = endPoint)
		{ }
		
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
		
		public async Task<GraphQLHttpResponse<TResponse>> SendHttpQueryAsync<TVariable, TResponse>(GraphQLHttpRequest<TVariable> request, CancellationToken cancellationToken = default) {
			using var httpRequestMessage = this.GenerateHttpRequestMessage(request);
			using var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);
			if (!httpResponseMessage.IsSuccessStatusCode) {
				throw new GraphQLHttpException(httpResponseMessage);
			}

			var bodyStream = await httpResponseMessage.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync<GraphQLHttpResponse<TResponse>>(bodyStream, this.Options.JsonSerializerOptions, cancellationToken);
		}

		public async Task<GraphQLHttpResponse<TResponse>> SendHttpQueryAsync<TResponse>(GraphQLHttpRequest request, CancellationToken cancellationToken = default) =>
			await this.SendHttpQueryAsync<dynamic, TResponse>(request, cancellationToken);

		public async Task<GraphQLHttpResponse<TResponse>> SendHttpMutationAsync<TVariable, TResponse>(GraphQLHttpRequest<TVariable> request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TVariable, TResponse>(GraphQLRequest<TVariable> request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<TResponse>> SendMutationAsync<TVariable, TResponse>(GraphQLRequest<TVariable> request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		private HttpRequestMessage GenerateHttpRequestMessage<T>(GraphQLRequest<T> request) {
			return new HttpRequestMessage(HttpMethod.Post, this.Options.EndPoint) {
				Content = new StringContent(JsonSerializer.Serialize(request, this.Options.JsonSerializerOptions), Encoding.UTF8, "application/json")
			};
		}

		public async Task<GraphQLResponse<R>> SendQueryAsync<R>(GraphQLRequest request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<R>> SendMutationAsync<R>(GraphQLRequest request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}
		public async Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			await Task.CompletedTask;
			throw new NotImplementedException();
		}

		private Uri GetWebSocketUri() {
			var webSocketSchema = this.Options.EndPoint.Scheme == "https" ? "wss" : "ws";
			return new Uri($"{webSocketSchema}://{this.Options.EndPoint.Host}:{this.Options.EndPoint.Port}{this.Options.EndPoint.AbsolutePath}");
		}

		/// <inheritdoc />
		public IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request) {
			if (disposed)
				throw new ObjectDisposedException(nameof(GraphQLHttpClient));

			if (subscriptionStreams.ContainsKey(request))
				return subscriptionStreams[request];

			var observable = graphQlHttpWebSocket.CreateSubscriptionStream(request, Options, cancellationToken: cancellationTokenSource.Token);

			subscriptionStreams.TryAdd(request, observable);
			return observable;
		}

		/// <inheritdoc />
		public IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request, Action<WebSocketException> webSocketExceptionHandler) {
			if (disposed)
				throw new ObjectDisposedException(nameof(GraphQLHttpClient));

			return CreateSubscriptionStream(request, e => {
				if (e is WebSocketException webSocketException)
					webSocketExceptionHandler(webSocketException);
				else
					throw e;
			});
		}

		/// <inheritdoc />
		public IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request, Action<Exception> exceptionHandler) {
			if (disposed)
				throw new ObjectDisposedException(nameof(GraphQLHttpClient));

			if (subscriptionStreams.ContainsKey(request))
				return subscriptionStreams[request];

			var observable = graphQlHttpWebSocket.CreateSubscriptionStream(request, Options, exceptionHandler, cancellationTokenSource.Token);
			subscriptionStreams.TryAdd(request, observable);
			return observable;
		}

		private readonly ConcurrentDictionary<GraphQLRequest, IObservable<GraphQLResponse>> subscriptionStreams = new ConcurrentDictionary<GraphQLRequest, IObservable<GraphQLResponse>>();


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
