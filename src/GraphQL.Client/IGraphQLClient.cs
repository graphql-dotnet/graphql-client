using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expression;
using GraphQL.Common.Response;

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


		Task<GraphQLResponse<TResponse>> SendAsync<TType, TResponse>(GqlExpression<TType, TResponse> expression,
			CancellationToken cancellationToken = default)
			where TResponse : class;

		Task<GraphQLResponse<TResponse>> SendAsync<TType, TResponse, TArgs>(GqlExpression<TType, TResponse, TArgs> expression,
			TArgs args,
			CancellationToken cancellationToken = default)
			where TResponse : class;



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
