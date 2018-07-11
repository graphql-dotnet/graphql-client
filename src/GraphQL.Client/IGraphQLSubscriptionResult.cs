using System;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the Result of a subscription
	/// </summary>
	[Obsolete("EXPERIMENTAL")]
	public interface IGraphQLSubscriptionResult : IDisposable {

		/// <summary>
		/// Event triggered when a new Response is received
		/// </summary>
		event Action<GraphQLResponse> OnReceive;

		/// <summary>
		/// Last Response Received
		/// </summary>
		GraphQLResponse LastResponse { get; }

	}

}
