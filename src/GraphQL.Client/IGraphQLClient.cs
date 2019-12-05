using System;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {

	public interface IGraphQLClient : IDisposable {

		Task<GraphQLResponse<R>> SendQueryAsync<R>(GraphQLRequest request, CancellationToken cancellationToken = default);

		Task<GraphQLResponse<R>> SendMutationAsync<R>(GraphQLRequest request, CancellationToken cancellationToken = default);

	}

}
