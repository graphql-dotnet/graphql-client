using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client {

	public partial class GraphQLClient:IDisposable {

		public Uri EndPoint {
			get => this.Options.EndPoint;
			set => this.Options.EndPoint = value;
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
			if (this.Options.HttpClientHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpClientHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.httpClient = new HttpClient(this.Options.HttpClientHandler);
		}

		#endregion

		public async Task<GraphQLResponse> GetQueryAsync(string query) =>
			await this.GetAsync(new GraphQLRequest { Query = query }).ConfigureAwait(false);

		public async Task<GraphQLResponse> GetAsync(GraphQLRequest query) {
			var queryParamsBuilder = new StringBuilder($"query={query.Query}", 3);
			if (query.OperationName != null) { queryParamsBuilder.Append($"&operationName={query.OperationName}"); }
			if (query.Variables != null) { queryParamsBuilder.Append($"&variables={JsonConvert.SerializeObject(query.Variables)}"); }
			var httpResponseMessage = await this.httpClient.GetAsync($"{this.Options.EndPoint}?{queryParamsBuilder.ToString()}").ConfigureAwait(false);
			return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
		}

		public async Task<GraphQLResponse> PostQueryAsync(string query) =>
			await this.PostAsync(new GraphQLRequest { Query = query }).ConfigureAwait(false);

		public async Task<GraphQLResponse> PostAsync(GraphQLRequest query) {
			var graphQLString = JsonConvert.SerializeObject(query, this.Options.JsonSerializerSettings);
			var httpContent = new StringContent(graphQLString, Encoding.UTF8, this.Options.MediaType);
			var httpResponseMessage = await this.httpClient.PostAsync(this.EndPoint, httpContent).ConfigureAwait(false);
			return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
		}

		/// <summary>
		/// Reads the <see cref="HttpResponseMessage"/>
		/// </summary>
		/// <param name="httpResponseMessage">The Response</param>
		/// <returns>The GrahQLResponse</returns>
		private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage) {
			var resultString = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<GraphQLResponse>(resultString, this.Options.JsonSerializerSettings);
		}

		public void Dispose() =>
			this.httpClient.Dispose();

	}

}
