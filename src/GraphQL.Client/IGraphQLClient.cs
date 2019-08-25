using System;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the Interface that any GraphQL Client must implement
	/// </summary>
	public interface IGraphQLClient : IDisposable {

		/// <summary>
		/// Send a Query async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Response</returns>
		Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Send a Mutation Async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Response</returns>
		Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Send a Subscription async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Subscription Response</returns>
		[Obsolete("EXPERIMENTAL")]
		Task<IGraphQLSubscriptionResult> SendSubscribeAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

	}

}
