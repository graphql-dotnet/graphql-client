using Xunit;

namespace GraphQL.Common.Tests.Request {

	public class GraphQLRequestTests {

		[Fact]
		public void FieldsRequest1Fact() {
			var graphQLRequest = GraphQLRequestConsts.FieldsRequest1;
		}

		[Fact]
		public void FieldsRequest2Fact() {
			var graphQLRequest = GraphQLRequestConsts.FieldsRequest2;
		}

		[Fact]
		public void ArgumentsRequest1Fact() {
			var graphQLRequest = GraphQLRequestConsts.ArgumentsRequest1;
		}

		[Fact]
		public void ArgumentsRequest2Fact() {
			var graphQLRequest = GraphQLRequestConsts.ArgumentsRequest2;
		}

		[Fact]
		public void AliasesRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.AliasesRequest;
		}

		[Fact]
		public void FragmentsRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.FragmentsRequest;
		}

		[Fact]
		public void OperationNameRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.OperationNameRequest;
		}

		[Fact]
		public void VariablesRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.VariablesRequest;
			Assert.NotNull(graphQLRequest.Variables);
			Assert.Equal("JEDI", graphQLRequest.Variables.episode);
		}

		[Fact]
		public void DirectivesRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.DirectivesRequest;
			Assert.NotNull(graphQLRequest.Variables);
			Assert.Equal("JEDI", graphQLRequest.Variables.episode);
			Assert.Equal(false, graphQLRequest.Variables.withFriends);
		}

		[Fact]
		public void MutationsRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.MutationsRequest;
			Assert.NotNull(graphQLRequest.Variables);
			Assert.Equal("JEDI", graphQLRequest.Variables.ep);
			Assert.Equal(5, graphQLRequest.Variables.review.stars);
			Assert.Equal("This is a great movie!", graphQLRequest.Variables.review.commentary);
		}

		[Fact]
		public void InlineFragmentsRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.InlineFragmentsRequest;
			Assert.NotNull(graphQLRequest.Variables);
			Assert.Equal("JEDI", graphQLRequest.Variables.ep);
		}

		[Fact]
		public void MetaFieldsRequestFact() {
			var graphQLRequest = GraphQLRequestConsts.MetaFieldsRequest;
		}

	}

}
