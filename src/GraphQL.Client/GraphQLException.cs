using System;

namespace GraphQL.Client {

	public class GraphQLException : Exception {

		public GraphQLError[] Errors { get; }

		public GraphQLException(GraphQLError[] errors) : base(errors[0].Message) {
			this.Errors = errors;
		}

	}

}
