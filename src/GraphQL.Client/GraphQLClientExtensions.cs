using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {
	public static class GraphQLClientExtensions {


		public static Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(this IGraphQLClient client,
			string query, object? variables = null,
			string? operationName = null, CancellationToken cancellationToken = default) {
			return client.SendQueryAsync<TResponse>(new GraphQLRequest(query, variables, operationName), cancellationToken: cancellationToken);
		}
	}
}
