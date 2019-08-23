using GraphQL.Common.Response;
using Xunit;

namespace GraphQL.Common.Tests.Response {

	public class GraphQLErrorTest {

		[Fact]
		public void ConstructorFact() {
			var graphQLError = new GraphQLError("message");
			Assert.NotNull(graphQLError.Message);
			Assert.Null(graphQLError.Extensions);
			Assert.Null(graphQLError.Locations);
			Assert.Null(graphQLError.Path);
		}

		[Fact]
		public void Equality1Fact() {
			var graphQLError = new GraphQLError("message");
			Assert.Equal(graphQLError, graphQLError);
		}

		[Fact]
		public void Equality2Fact() {
			var graphQLError1 = new GraphQLError("message");
			var graphQLError2 = new GraphQLError("message");
			Assert.Equal(graphQLError1, graphQLError2);
		}

		[Fact]
		public void Equality3Fact() {
			var graphQLError1 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			var graphQLError2 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			Assert.Equal(graphQLError1, graphQLError2);
		}

		[Fact]
		public void EqualityOperatorFact() {
			var graphQLError1 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			var graphQLError2 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			Assert.True(graphQLError1 == graphQLError2);
		}

		[Fact]
		public void InEquality1Fact() {
			var graphQLError1 = new GraphQLError("message1");
			var graphQLError2 = new GraphQLError("message2");
			Assert.NotEqual(graphQLError1, graphQLError2);
		}

		[Fact]
		public void InEquality2Fact() {
			var graphQLError1 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			var graphQLError2 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 2, Line = 1 } } };
			Assert.NotEqual(graphQLError1, graphQLError2);
		}

		[Fact]
		public void InEqualityOperatorFact() {
			var graphQLError1 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			var graphQLError2 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 2, Line = 1 } } };
			Assert.True(graphQLError1 != graphQLError2);
		}

		[Fact]
		public void GetHashCodeFact() {
			var graphQLError1 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			var graphQLError2 = new GraphQLError("message") { Locations = new[] { new GraphQLLocation { Column = 1, Line = 2 } } };
			Assert.True(graphQLError1.GetHashCode() == graphQLError2.GetHashCode());
		}

	}

}
