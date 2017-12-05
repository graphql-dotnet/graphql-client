using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Client.Response;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client {

	public class GraphQLClient {

		public Uri EndPoint {
			get { return this.Options.EndPoint; }
			set { this.Options.EndPoint = value; }
		}

		public GraphQLClientOptions Options { get; set; } = new GraphQLClientOptions();

		private readonly HttpClient httpClient = new HttpClient();

		public GraphQLClient(Uri endPoint) {
			this.Options.EndPoint = endPoint;
		}

		public GraphQLClient(GraphQLClientOptions options) {
			this.Options = options;
		}

		public GraphQLClient(Uri endPoint, GraphQLClientOptions options) {
			this.Options = options;
			this.Options.EndPoint = endPoint;
		}

		public async Task<GraphQLResponse> GetQueryAsync(string query) {
			var httpResponseMessage = await this.httpClient.GetAsync($"{this.Options.EndPoint}?query={query}").ConfigureAwait(false);
			var resultString = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<GraphQLResponse>(resultString);
		}

		public async Task<GraphQLResponse> GetAsync(GraphQLQuery query) {
			await Task.FromResult(0);
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse> PostAsync(GraphQLQuery query) {
			var graphQLString = JsonConvert.SerializeObject(query, this.Options.JsonSerializerSettings);
			var httpContent = new StringContent(graphQLString, Encoding.UTF8, this.Options.MediaType);
			var httpResponse = await this.httpClient.PostAsync(this.EndPoint, httpContent).ConfigureAwait(false);
			var resultString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<GraphQLResponse>(resultString, this.Options.JsonSerializerSettings);
		}

	}

}
