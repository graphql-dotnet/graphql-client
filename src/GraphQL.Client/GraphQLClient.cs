using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Exceptions;
using GraphQL.Client.Experimental;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client {

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	public partial class GraphQLClient : IDisposable {

		#region Properties

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public HttpRequestHeaders DefaultRequestHeaders =>
			this.httpClient.DefaultRequestHeaders;

		/// <summary>
		/// The GraphQL EndPoint to be used
		/// </summary>
		public Uri EndPoint {
			get => this.Options.EndPoint;
			set => this.Options.EndPoint = value;
		}

		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLClientOptions Options { get; set; }

		#endregion

		private readonly HttpClient httpClient;

		#region Constructors

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(string endPoint) : this(new Uri(endPoint)) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(Uri endPoint) : this(new GraphQLClientOptions { EndPoint = endPoint }) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(string endPoint, GraphQLClientOptions options) : this(new Uri(endPoint), options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(Uri endPoint, GraphQLClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			this.Options.EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));

			if (this.Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(this.Options.JsonSerializerSettings)); }
			if (this.Options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpMessageHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.httpClient = new HttpClient(this.Options.HttpMessageHandler);
		}

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(GraphQLClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));

			if (this.Options.EndPoint == null) { throw new ArgumentNullException(nameof(this.Options.EndPoint)); }
			if (this.Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(this.Options.JsonSerializerSettings)); }
			if (this.Options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpMessageHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.httpClient = new HttpClient(this.Options.HttpMessageHandler);
		}

		#endregion

		#region GetQuery

		/// <summary>
		/// Send a query via GET
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> GetQueryAsync(string query, CancellationToken cancellationToken=default) {
			if (query == null) { throw new ArgumentNullException(nameof(query)); }

			return await this.GetAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via GET
		/// </summary>
		/// <param name="graphQLRequest">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> GetAsync(GraphQLRequest graphQLRequest, CancellationToken cancellationToken=default) {
			if (graphQLRequest == null) { throw new ArgumentNullException(nameof(graphQLRequest)); }
			if (graphQLRequest.Query == null) { throw new ArgumentNullException(nameof(graphQLRequest.Query)); }

			var queryParamsBuilder = new StringBuilder($"query={graphQLRequest.Query}", 3);
			if (graphQLRequest.OperationName != null) { queryParamsBuilder.Append($"&operationName={graphQLRequest.OperationName}"); }
			if (graphQLRequest.Variables != null) { queryParamsBuilder.Append($"&variables={JsonConvert.SerializeObject(graphQLRequest.Variables)}"); }
			using (var httpResponseMessage = await this.httpClient.GetAsync($"{this.Options.EndPoint}?{queryParamsBuilder.ToString()}", cancellationToken).ConfigureAwait(false)) {
				return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
			}
		}

		#endregion

		#region PostQuery

		/// <summary>
		/// Send a query via POST
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> PostQueryAsync(string query, CancellationToken cancellationToken=default) {
			if (query == null) { throw new ArgumentNullException(nameof(query)); }

			return await this.PostAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via POST
		/// </summary>
		/// <param name="graphQLRequest">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> PostAsync(GraphQLRequest graphQLRequest, CancellationToken cancellationToken=default) {
			if (graphQLRequest == null) { throw new ArgumentNullException(nameof(graphQLRequest)); }
			if (graphQLRequest.Query == null) { throw new ArgumentNullException(nameof(graphQLRequest.Query)); }

			var graphQLString = JsonConvert.SerializeObject(graphQLRequest, this.Options.JsonSerializerSettings);
			using (var httpContent = new StringContent(graphQLString, Encoding.UTF8, this.Options.MediaType.MediaType))
			using (var httpResponseMessage = await this.httpClient.PostAsync(this.EndPoint, httpContent, cancellationToken).ConfigureAwait(false)) {
				return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
			}
		}

		#endregion

		#region SubscribeQuery

		/// <summary>
		/// Subscribes to a GraphQLQuery
		/// </summary>
		/// <param name="query"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Obsolete("EXPERIMENTAL")]
		public async Task<GraphQLSubscriptionResult> SubscribeAsync(string query, CancellationToken cancellationToken = default) {
			if (query == null) { throw new ArgumentNullException(nameof(query)); }

			return await this.SubscribeAsync(new GraphQLRequest { Query = query }).ConfigureAwait(false);
		}

		/// <summary>
		/// Subscribes to a GraphQLQuery
		/// </summary>
		/// <param name="graphQLRequest"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Obsolete("EXPERIMENTAL")]
		public async Task<GraphQLSubscriptionResult> SubscribeAsync(GraphQLRequest graphQLRequest, CancellationToken cancellationToken = default) {
			if (graphQLRequest == null) { throw new ArgumentNullException(nameof(graphQLRequest)); }
			if (graphQLRequest.Query == null) { throw new ArgumentNullException(nameof(graphQLRequest.Query)); }

			var wsEndPoint = new Uri($"{this.Options.EndPoint}");
			var graphQLSubscriptionResult = new GraphQLSubscriptionResult(wsEndPoint, cancellationToken);
			await graphQLSubscriptionResult.StartAsync().ConfigureAwait(false);
			return graphQLSubscriptionResult;
		}

		#endregion

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose() =>
			this.httpClient.Dispose();

		/// <summary>
		/// Reads the <see cref="HttpResponseMessage"/>
		/// </summary>
		/// <param name="httpResponseMessage">The Response</param>
		/// <returns>The GrahQLResponse</returns>
		private async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage) {
			using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
			using (var streamReader = new StreamReader(stream))
			using (var jsonTextReader = new JsonTextReader(streamReader)) {
				var jsonSerializer = new JsonSerializer {
					ContractResolver = this.Options.JsonSerializerSettings.ContractResolver
				};
				try {
					return jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
				}
				catch (JsonReaderException exception) {
					if (httpResponseMessage.IsSuccessStatusCode) {
						throw exception;
					}
					throw new GraphQLHttpException(httpResponseMessage);
				}
			}
		}

	}

}
