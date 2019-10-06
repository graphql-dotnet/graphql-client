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

		public async Task<GraphQLHttpResponse<TResponse>> SendHttpQueryAsync<TVariable, TResponse>(GraphQLHttpRequest<TVariable> request, CancellationToken cancellationToken = default) {
			using var httpRequestMessage = this.GenerateHttpRequestMessage(request);
			using var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);
			if (!httpResponseMessage.IsSuccessStatusCode) {
				throw new GraphQLHttpException(httpResponseMessage);
			}
			var httpBody = await httpRequestMessage.Content.ReadAsStringAsync();
			var graphQLHttpResponse=JsonSerializer.Deserialize<GraphQLHttpResponse<TResponse>>(httpBody, this.JsonSerializerOptions);
			throw new NotImplementedException();
		}

		public async Task<GraphQLHttpResponse<TResponse>> SendHttpQueryAsync<TResponse>(GraphQLHttpRequest request, CancellationToken cancellationToken = default) =>
			await this.SendHttpQueryAsync<dynamic, TResponse>(request, cancellationToken);

		public async Task<GraphQLHttpResponse<TResponse>> SendHttpMutationAsync<TVariable, TResponse>(GraphQLHttpRequest<TVariable> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TVariable, TResponse>(GraphQLRequest<TVariable> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse<TResponse>> SendMutationAsync<TVariable, TResponse>(GraphQLRequest<TVariable> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		private HttpRequestMessage GenerateHttpRequestMessage<T>(GraphQLRequest<T> request) {
			return new HttpRequestMessage(HttpMethod.Post, this.EndPoint) {
				Content = new StringContent(JsonSerializer.Serialize(request, this.JsonSerializerOptions), Encoding.UTF8, "application/json")
			};
		}

	}

}
