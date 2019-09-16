using System;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {

	public interface IGraphQLClient : IDisposable {

		Task<GraphQLResponse<R>> SendQueryAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default);

		Task<GraphQLResponse<R>> SendMutationAsync<V, R>(GraphQLRequest<V> request, CancellationToken cancellationToken = default);

	}

}
