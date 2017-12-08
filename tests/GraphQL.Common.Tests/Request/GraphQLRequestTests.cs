using Xunit;

namespace GraphQL.Common.Tests.Request {

	public class GraphQLRequestTests {

		[Fact]
		public void SchemaTypeNameQueryFact() {
			var graphQLRequest = GraphQLRequestConsts.SchemaTypeNameRequest;
			AssertGraphQL.CorrectGraphQLRequest(graphQLRequest);
		}

		[Fact]
		public void PokemonPikachuQueryFact() {
			var graphQLRequest = GraphQLRequestConsts.PokemonPikachuRequest;
			AssertGraphQL.CorrectGraphQLRequest(graphQLRequest);
		}

		[Fact]
		public void HeroNameAndFriendsRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.HeroNameAndFriendsRequest;
			AssertGraphQL.CorrectGraphQLRequest(graphQLRequest);
			Assert.NotNull(graphQLRequest.Variables);
			Assert.Equal("JEDI", graphQLRequest.Variables.episode);
		}

	}

}
