using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Common.Tests {

	/// <summary>
	/// Represents the union of a <see cref="GraphQLRequest"/> with its <see cref="GraphQLResponse"/>
	/// </summary>
	public class GraphQLRequestResponse{

		/// <summary>
		/// The Request
		/// </summary>
		public GraphQLRequest Request { get; set; }

		/// <summary>
		/// The Response
		/// </summary>
		public GraphQLResponse Response { get; set; }

	}

}
