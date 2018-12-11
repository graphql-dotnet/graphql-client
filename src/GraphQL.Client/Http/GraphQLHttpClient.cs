using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http.Internal;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Client.Http {

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	public partial class GraphQLHttpClient : IGraphQLClient {

		#region Properties

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public HttpRequestHeaders DefaultRequestHeaders => this.graphQLHttpHandler.HttpClient.DefaultRequestHeaders;

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
		public GraphQLHttpClientOptions Options {
			get => this.graphQLHttpHandler.Options;
			set => this.graphQLHttpHandler.Options = value;
		}

		#endregion

		internal readonly GraphQLHttpHandler graphQLHttpHandler;

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLHttpClient(string endPoint) : this(new Uri(endPoint)) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLHttpClient(Uri endPoint) : this(new GraphQLHttpClientOptions { EndPoint = endPoint }) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(string endPoint, GraphQLHttpClientOptions options) : this(new Uri(endPoint), options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(Uri endPoint, GraphQLHttpClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			options.EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
		}

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(GraphQLHttpClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
		}

		internal GraphQLHttpClient(GraphQLHttpClientOptions options, HttpClient httpClient) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.graphQLHttpHandler = new GraphQLHttpHandler(options, httpClient);
		}

		public Task<GraphQLResponse> SendQueryAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendQueryAsync(new GraphQLRequest(query), cancellationToken);

		public Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.PostAsync(request, cancellationToken);

		public Task<GraphQLResponse> SendMutationAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendMutationAsync(new GraphQLRequest(query), cancellationToken);

		public Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.PostAsync(request, cancellationToken);

		[Obsolete("EXPERIMENTAL API")]
		public Task<IGraphQLSubscriptionResult> SendSubscribeAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendSubscribeAsync(new GraphQLRequest(query), cancellationToken);

		[Obsolete("EXPERIMENTAL API")]
		public Task<IGraphQLSubscriptionResult> SendSubscribeAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			var webSocketSchema = this.EndPoint.Scheme == "https" ? "wss" : "ws";
			var webSocketUri = new Uri($"{webSocketSchema}://{this.EndPoint.Host}:{this.EndPoint.Port}{this.EndPoint.AbsolutePath}");
			var graphQLSubscriptionResult = new GraphQLHttpSubscriptionResult(webSocketUri, request);
			graphQLSubscriptionResult.StartAsync(cancellationToken);
			return Task.FromResult<IGraphQLSubscriptionResult>(graphQLSubscriptionResult);
		}

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose() =>
			this.graphQLHttpHandler.Dispose();

	}

}
