using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	[Obsolete("Use GraphQLHttpClient directly")]
	public class GraphQLClient : GraphQLHttpClient {

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(string endPoint) : base(endPoint) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(Uri endPoint) : base(endPoint) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(string endPoint, GraphQLClientOptions options) : base(endPoint, options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(Uri endPoint, GraphQLClientOptions options) : base(endPoint, options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(GraphQLClientOptions options) : base(options) { }


		/// <summary>
		/// Send a query via GET
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> GetQueryAsync(string query, CancellationToken cancellationToken = default) =>
			this.GetAsync(new GraphQLRequest(query), cancellationToken);

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via GET
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> GetAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.GetAsync(request, cancellationToken);

		/// <summary>
		/// Send a query via POST
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> PostQueryAsync(string query, CancellationToken cancellationToken = default) =>
			this.PostAsync(new GraphQLRequest(query), cancellationToken);

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via POST
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public Task<GraphQLResponse> PostAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.PostAsync(request, cancellationToken);

	}

}
