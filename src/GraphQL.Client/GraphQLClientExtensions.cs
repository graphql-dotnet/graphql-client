using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {
	public static class GraphQLClientExtensions {
		public static Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse, TVariables>(this IGraphQLClient client,
			string query, TVariables variables,
			string? operationName = null, CancellationToken cancellationToken = default) {
			return client.SendQueryAsync<TResponse>(GraphQLRequest.New(query, variables, operationName), cancellationToken);
		}

		public static Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(this IGraphQLClient client,
			string query, string? operationName = null, CancellationToken cancellationToken = default) {
			return client.SendQueryAsync<TResponse>(GraphQLRequest.New(query, operationName), cancellationToken);
		}


		public static Task<GraphQLResponse> SendQueryAsync(this IGraphQLClient client,
			string query, string? operationName = null, CancellationToken cancellationToken = default) {
			return client.SendQueryAsync(GraphQLRequest.New(query, operationName), cancellationToken);
		}
	}
}
