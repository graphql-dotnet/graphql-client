using System;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the Result of a subscription
	/// </summary>
	[Obsolete("EXPERIMENTAL")]
	public interface IGraphQLSubscriptionResult<T> : IDisposable {

		/// <summary>
		/// Event triggered when a new Response is received
		/// </summary>
		event Action<GraphQLResponse<T>> OnReceive;

		/// <summary>
		/// Last Response Received
		/// </summary>
		GraphQLResponse<T> LastResponse { get; }

	}

}
