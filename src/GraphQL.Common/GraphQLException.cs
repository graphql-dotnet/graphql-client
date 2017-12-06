using System;
using GraphQL.Common.Response;

namespace GraphQL.Common {

	public class GraphQLException : Exception {

		public GraphQLError GraphQLError { get; set; }

	}

}
