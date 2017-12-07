using GraphQL.Common.Response;
using Xunit;

namespace GraphQL.Client.Tests {

	public static class AssertGraphQL {

		public static void CorrectGraphQLResponse(GraphQLResponse graphQLResponse) {
			Assert.NotNull(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

	}

}
