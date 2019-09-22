using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Http {

	public class GraphQLHttpClient : IDisposable, IGraphQLClient {

		public Uri EndPoint { get; set; }

		public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		private readonly HttpClient httpClient;

		public GraphQLHttpClient(string endPoint) {
			this.EndPoint = new Uri(endPoint);
			this.httpClient = new HttpClient();
		}

		public GraphQLHttpClient(Uri endPoint) {
			this.EndPoint = endPoint;
			this.httpClient = new HttpClient();
		}

		public GraphQLHttpClient(string endPoint, GraphQLHttpClientOptions options) {
			this.EndPoint = new Uri(endPoint);
			this.httpClient = new HttpClient();
		}

		public GraphQLHttpClient(Uri endPoint, GraphQLHttpClientOptions options) {
			this.EndPoint = endPoint;
			this.httpClient = new HttpClient();
		}

		public GraphQLHttpClient(string endPoint, HttpClient httpClient) {
			this.EndPoint = new Uri(endPoint);
			this.httpClient = httpClient;
		}

		public GraphQLHttpClient(Uri endPoint, HttpClient httpClient) {
			this.EndPoint = endPoint;
			this.httpClient = httpClient;
		}

		public GraphQLHttpClient(string endPoint, GraphQLHttpClientOptions options, HttpClient httpClient) {
			this.EndPoint = new Uri(endPoint);
			this.httpClient = httpClient;
		}

		public GraphQLHttpClient(Uri endPoint, GraphQLHttpClientOptions options, HttpClient httpClient) {
			this.EndPoint = endPoint;
			this.httpClient = httpClient;
		}

		public void Dispose() => this.httpClient.Dispose();

		public async Task<GraphQLHttpResponse<R>> SendHttpQueryAsync<V, R>(GraphQLHttpRequest<V> request, CancellationToken cancellationToken = default) {
			using var httpRequestMessage = await this.GenerateHttpRequestMessageAsync(request);
			using var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);
			throw new NotImplementedException();
		}

		public async Task<GraphQLHttpResponse<R>> SendHttpMutationAsync<V, R>(GraphQLHttpRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<R>> SendQueryAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<R>> SendMutationAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		private async Task<HttpRequestMessage> GenerateHttpRequestMessageAsync<T>(GraphQLRequest<T> request, CancellationToken cancellationToken = default) {
			return new HttpRequestMessage(HttpMethod.Post, this.EndPoint) {
				Content = new StringContent(JsonSerializer.Serialize(request, this.JsonSerializerOptions), Encoding.UTF8, "application/json")
			};
		}

	}

}
