using GraphQL.Common.Response;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Common.Tests.Response {

	public class GraphQLResponseTest {

		[Fact]
		public void Constructor1Fact() {
			var graphQLResponse = new GraphQLResponse();
			Assert.Null(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

		[Fact]
		public void Constructor2Fact() {
			var graphQLResponse = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			Assert.NotNull(graphQLResponse.Data);
			Assert.NotNull(graphQLResponse.Errors);
		}

		[Fact]
		public void Equality1Fact() {
			var graphQLResponse = new GraphQLResponse();
			Assert.Equal(graphQLResponse, graphQLResponse);
		}

		[Fact]
		public void Equality2Fact() {
			var graphQLResponse1 = new GraphQLResponse();
			var graphQLResponse2 = new GraphQLResponse();
			Assert.Equal(graphQLResponse1, graphQLResponse2);
		}

		[Fact]
		public void Equality3Fact() {
			var graphQLResponse1 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			var graphQLResponse2 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			Assert.Equal(graphQLResponse1, graphQLResponse2);
		}

		[Fact]
		public void EqualityOperatorFact() {
			var graphQLResponse1 = new GraphQLResponse();
			var graphQLResponse2 = new GraphQLResponse();
			Assert.True(graphQLResponse1 == graphQLResponse2);
		}

		[Fact]
		public void InEqualityFact() {
			var graphQLResponse1 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			var graphQLResponse2 = new GraphQLResponse {
				Data = new { a = 2 },
				Errors = new[] { new GraphQLError("message") }
			};
			Assert.NotEqual(graphQLResponse1, graphQLResponse2);
		}

		[Fact]
		public void InEqualityOperatorFact() {
			var graphQLResponse1 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			var graphQLResponse2 = new GraphQLResponse {
				Data = new { a = 2 },
				Errors = new[] { new GraphQLError("message") }
			};
			Assert.True(graphQLResponse1 != graphQLResponse2);
		}

		[Fact]
		public void GetHashCodeFact() {
			var graphQLResponse1 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			var graphQLResponse2 = new GraphQLResponse {
				Data = new { a = 1 },
				Errors = new[] { new GraphQLError("message") }
			};
			Assert.True(graphQLResponse1.GetHashCode() == graphQLResponse2.GetHashCode());
		}

		[Fact]
		public void GetDataFieldAsFact() {
			var graphQLResponse1 = new GraphQLResponse {
				Data = new {
					hero = new Person { Name = "R2-D2" }
				},
			};
			Assert.Equal("R2-D2", graphQLResponse1.GetDataFieldAs<Person>("hero").Name);
		}

	}

}
