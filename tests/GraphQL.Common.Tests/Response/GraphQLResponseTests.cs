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
			var graphQLResponse = GraphQLResponseConsts.AliasesResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("Luke Skywalker", graphQLResponse.Data.empireHero.name.Value);
			Assert.Equal("R2-D2", graphQLResponse.Data.jediHero.name.Value);
		}

		[Fact]
		public void FragmentsResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.FragmentsResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("Luke Skywalker", graphQLResponse.Data.leftComparison.name.Value);
			Assert.Equal("NEWHOPE", graphQLResponse.Data.leftComparison.appearsIn[0].Value);
			Assert.Equal("EMPIRE", graphQLResponse.Data.leftComparison.appearsIn[1].Value);
			Assert.Equal("JEDI", graphQLResponse.Data.leftComparison.appearsIn[2].Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.leftComparison.friends[0].name.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.leftComparison.friends[1].name.Value);
			Assert.Equal("C-3PO", graphQLResponse.Data.leftComparison.friends[2].name.Value);
			Assert.Equal("R2-D2", graphQLResponse.Data.leftComparison.friends[3].name.Value);

			Assert.Equal("R2-D2", graphQLResponse.Data.rightComparison.name.Value);
			Assert.Equal("NEWHOPE", graphQLResponse.Data.rightComparison.appearsIn[0].Value);
			Assert.Equal("EMPIRE", graphQLResponse.Data.rightComparison.appearsIn[1].Value);
			Assert.Equal("JEDI", graphQLResponse.Data.rightComparison.appearsIn[2].Value);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.rightComparison.friends[0].name.Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.rightComparison.friends[1].name.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.rightComparison.friends[2].name.Value);
		}

		[Fact]
		public void OperationNameResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.OperationNameResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.hero.friends[0].name.Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.hero.friends[1].name.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.hero.friends[2].name.Value);
		}

		[Fact]
		public void VariablesResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.VariablesResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
			Assert.Equal("Luke Skywalker", graphQLResponse.Data.hero.friends[0].name.Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.hero.friends[1].name.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.hero.friends[2].name.Value);
		}

		[Fact]
		public void DirectivesResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.DirectivesResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
		}

		[Fact]
		public void MutationsResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.MutationsResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal(5, graphQLResponse.Data.createReview.stars.Value);
			Assert.Equal("This is a great movie!", graphQLResponse.Data.createReview.commentary.Value);
		}

		[Fact]
		public void InlineFragmentsResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.InlineFragmentsResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("R2-D2", graphQLResponse.Data.hero.name.Value);
			Assert.Equal("Astromech", graphQLResponse.Data.hero.primaryFunction.Value);
		}

		[Fact]
		public void MetaFieldsResponseFact() {
			var graphQLResponse = GraphQLResponseConsts.MetaFieldsResponse;
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);

			Assert.Equal("Human", graphQLResponse.Data.search[0].__typename.Value);
			Assert.Equal("Han Solo", graphQLResponse.Data.search[0].name.Value);
			Assert.Equal("Human", graphQLResponse.Data.search[1].__typename.Value);
			Assert.Equal("Leia Organa", graphQLResponse.Data.search[1].name.Value);
			Assert.Equal("Starship", graphQLResponse.Data.search[2].__typename.Value);
			Assert.Equal("TIE Advanced x1", graphQLResponse.Data.search[2].name.Value);
		}

	}

}
