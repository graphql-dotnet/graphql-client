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

		private const string GraphQLMediaType= "application/json"; // This should be application/graphql

		public Uri EndPoint {
			get { return this.httpClient.BaseAddress; }
			set { this.httpClient.BaseAddress = value; }
		}

		private readonly HttpClient httpClient = new HttpClient();

		public GraphQLClient(Uri endpoint) {
			this.EndPoint = endpoint;
		}

		public async Task<GraphQLResponse> GetQueryAsync(string query) {
			var httpResponseMessage = await this.httpClient.GetAsync($"{this.httpClient.BaseAddress}?query={query}").ConfigureAwait(false);
			var resultString = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<GraphQLResponse>(resultString);
		}

		public async Task<GraphQLResponse> GetAsync(GraphQLQuery query) {
			await Task.FromResult(0);
			throw new NotImplementedException();
		}

		public async Task<GraphQLResponse> PostAsync(GraphQLQuery query) {
			var graphQLString = JsonConvert.SerializeObject(query,new JsonSerializerSettings {ContractResolver=new CamelCasePropertyNamesContractResolver() });
			var httpContent = new StringContent(graphQLString, Encoding.UTF8, GraphQLMediaType);
			var httpResponse=await this.httpClient.PostAsync(this.EndPoint, httpContent).ConfigureAwait(false);
			var resultString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<GraphQLResponse>(resultString, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
		}

	}

}
