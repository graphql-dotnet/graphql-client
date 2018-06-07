using System;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	public interface IGraphQLSubscriptionResult : IDisposable {

		event Action<GraphQLResponse> OnReceive;

		GraphQLResponse LastResponse { get; }

	}

}
