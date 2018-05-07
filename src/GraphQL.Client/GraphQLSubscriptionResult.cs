using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the result of a subscription query
	/// </summary>
	[Obsolete("EXPERIMENTAL API")]
	public class GraphQLSubscriptionResult {

		public event Action<GraphQLResponse> OnReceive;

		public GraphQLResponse LastResponse { get; }

		public GraphQLSubscriptionResult() {
			this.OnReceive.Invoke(null);
		}

	}

}
