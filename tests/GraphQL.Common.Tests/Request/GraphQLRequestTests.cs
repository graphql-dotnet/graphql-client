using Xunit;

namespace GraphQL.Common.Tests.Request {

	public class GraphQLRequestTests {

		[Fact]
		public void SchemaTypeNameQueryFact() {
			var graphQLRequest = GraphQLRequestConsts.SchemaTypeNameQuery;
			AssertGraphQL.CorrectGraphQLRequest(graphQLRequest);
		}

		[Fact]
		public void PokemonPikachuQueryFact() {
			var graphQLRequest = GraphQLRequestConsts.PokemonPikachuQuery;
			AssertGraphQL.CorrectGraphQLRequest(graphQLRequest);
		}

	}

}
