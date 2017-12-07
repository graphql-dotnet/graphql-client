using System;
using GraphQL.Common.Response;

namespace GraphQL.Common.Exceptions {

	public class GraphQLException : Exception {

		public GraphQLError GraphQLError { get; set; }

	}

}
