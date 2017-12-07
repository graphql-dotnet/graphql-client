using GraphQL.Common.Response;
using Xunit;

namespace GraphQL.Common.Tests {

	public static class AssertGraphQL {

		public static void CorrectGraphQLResponse(GraphQLResponse graphQLResponse) {
			Assert.NotNull(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

	}

}
