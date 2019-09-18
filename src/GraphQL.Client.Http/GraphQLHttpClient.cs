using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Http {

	public class GraphQLHttpClient : IDisposable, IGraphQLClient {

		public Uri EndPoint { get; set; }

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

		public Task<GraphQLResponse<R>> SendQueryAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<GraphQLResponse<R>> SendMutationAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<GraphQLHttpResponse<R>> SendQueryAsync<V, R>(GraphQLHttpRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

		public Task<GraphQLHttpResponse<R>> SendMutationAsync<V, R>(GraphQLHttpRequest<V> request, CancellationToken cancellationToken = default) {
			throw new NotImplementedException();
		}

	}

}
