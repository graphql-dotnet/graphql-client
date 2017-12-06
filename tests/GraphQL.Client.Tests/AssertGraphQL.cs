using System;
using GraphQL.Client.Response;
using Xunit;

namespace GraphQL.Client.Tests {

	public static class AssertGraphQL {

		public static void CorrectGraphQLResponse(GraphQLResponse graphQLResponse) {
			if (graphQLResponse == null) { throw new ArgumentNullException(nameof(graphQLResponse)); }
			Assert.NotNull(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

	}

}
