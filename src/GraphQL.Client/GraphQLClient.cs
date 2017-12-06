using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Client.Response;
using Newtonsoft.Json;
using System.Text;

namespace GraphQL.Client {

	public partial class GraphQLClient {

		public Uri EndPoint {
			get { return this.Options.EndPoint; }
			set { this.Options.EndPoint = value; }
		}

		public GraphQLClientOptions Options { get; set; }

		private readonly HttpClient httpClient;

		#region Constructors

		public GraphQLClient(string endPoint) : this(new Uri(endPoint)) { }

		public GraphQLClient(Uri endPoint) : this(new GraphQLClientOptions { EndPoint = endPoint }) { }

		public GraphQLClient(string endPoint, GraphQLClientOptions options) : this(new Uri(endPoint), options) { }

		public GraphQLClient(Uri endPoint, GraphQLClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			this.Options.EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));

			if (this.Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(this.Options.JsonSerializerSettings)); }
			if (this.Options.HttpClientHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpClientHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.httpClient = new HttpClient(this.Options.HttpClientHandler);
		}

		public GraphQLClient(GraphQLClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));

			if (this.Options.EndPoint == null) { throw new ArgumentNullException(nameof(this.Options.EndPoint)); }
			if (this.Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(this.Options.JsonSerializerSettings)); }
			if(this.Options.HttpClientHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpClientHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.httpClient = new HttpClient(this.Options.HttpClientHandler);
		}

		#endregion

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
