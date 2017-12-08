using Xunit;

namespace GraphQL.Common.Tests.Response {

	public class GraphQLResponseTests {

		[Fact]
		public void FieldsResponse1Fact() {
			var graphQLResponse = GraphQLResponseConsts.FieldsResponse1;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
		}

		[Fact]
		public void FieldsResponse2Fact() {
			var graphQLResponse = GraphQLResponseConsts.FieldsResponse2;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.hero.friends[0].name.Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.hero.friends[1].name.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.hero.friends[2].name.Value);
		}

		[Fact]
		public void ArgumentsResponse1Fact() {
			var graphQLResponse = GraphQLResponseConsts.ArgumentsResponse1;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.human.name.Value);
			Assert.Equal(1.72, graphQLResponse.Data.human.height.Value);
		}

		[Fact]
		public void ArgumentsResponse2Fact() {
			var graphQLResponse = GraphQLResponseConsts.ArgumentsResponse2;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.human.name.Value);
			Assert.Equal(5.6430448, graphQLResponse.Data.human.height.Value);
		}

		[Fact]
		public void AliasesResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.ArgumentsResponse2;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.empireHero.name.Value);
			Assert.Equal("R2-D2", graphQLResponse.Data.jediHero.name.Value);
		}

	}

}
