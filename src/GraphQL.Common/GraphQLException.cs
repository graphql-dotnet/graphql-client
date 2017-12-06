using System;
using GraphQL.Client.Response;

namespace GraphQL.Common {

	public class GraphQLException : Exception {

		public GraphQLError GraphQLError { get; set; }

	}

}
