using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.Client {

    public partial class GraphQLClient {

        /// <summary>
		/// Send a query via GET
		/// </summary>
		/// <param name="query">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		[Obsolete("Use SendQueryAsync or SendMutationAsync")]
        public async Task<GraphQLResponse> GetQueryAsync(string query, CancellationToken cancellationToken = default) =>
            await this.GetAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Send a <see cref="GraphQLRequest"/> via GET
        /// </summary>
        /// <param name="request">The Request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Response</returns>
        [Obsolete("Use SendQueryAsync or SendMutationAsync")]
        public async Task<GraphQLResponse> GetAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
            await this.graphQLHttpHandler.GetAsync(request, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Send a query via POST
        /// </summary>
        /// <param name="query">The Request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Response</returns>
        [Obsolete("Use SendQueryAsync or SendMutationAsync")]
        public async Task<GraphQLResponse> PostQueryAsync(string query, CancellationToken cancellationToken = default) =>
            await this.PostAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Send a <see cref="GraphQLRequest"/> via POST
        /// </summary>
        /// <param name="request">The Request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Response</returns>
        [Obsolete("Use SendQueryAsync or SendMutationAsync")]
        public async Task<GraphQLResponse> PostAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
            await this.graphQLHttpHandler.PostAsync(request, cancellationToken).ConfigureAwait(false);

    }

}
