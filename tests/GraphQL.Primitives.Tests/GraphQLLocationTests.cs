using Xunit;

namespace GraphQL.Primitives.Tests {

	public class GraphQLLocationTests {

		[Fact]
		public void Constructor1() {
			var graphQLLocation = new GraphQLLocation();
			Assert.NotNull(graphQLLocation);
		}

		[Fact]
		public void Constructor2() {
			var graphQLLocation = new GraphQLLocation {
				Column = 10
			};
			Assert.Equal(10U, graphQLLocation.Column);
		}

		[Fact]
		public void Constructor3() {
			var graphQLLocation = new GraphQLLocation {
				Line = 10
			};
			Assert.Equal(10U, graphQLLocation.Line);
		}

		[Fact]
		public void Constructor4() {
			var graphQLLocation = new GraphQLLocation {
				Column = 10,
				Line = 10
			};
			Assert.Equal(10U, graphQLLocation.Column);
			Assert.Equal(10U, graphQLLocation.Line);
		}

		[Fact]
		public void Equality1() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation();
			Assert.True(graphQLLocation1.Equals(graphQLLocation2));
			Assert.True(graphQLLocation2.Equals(graphQLLocation1));
		}

		[Fact]
		public void Equality2() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			Assert.True(graphQLLocation1.Equals(graphQLLocation2));
			Assert.True(graphQLLocation2.Equals(graphQLLocation1));
		}

		[Fact]
		public void Equality3() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation();
			Assert.True(graphQLLocation1 == graphQLLocation2);
			Assert.True(graphQLLocation2 == graphQLLocation1);
		}

		[Fact]
		public void Equality4() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			Assert.True(graphQLLocation1 == graphQLLocation2);
			Assert.True(graphQLLocation2 == graphQLLocation1);
		}

		[Fact]
		public void Equality5() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation();
			Assert.True(graphQLLocation1.Equals((object)graphQLLocation2));
			Assert.True(graphQLLocation2.Equals((object)graphQLLocation1));
		}

		[Fact]
		public void Equality6() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			Assert.True(graphQLLocation1.Equals((object)graphQLLocation2));
			Assert.True(graphQLLocation2.Equals((object)graphQLLocation1));
		}

		[Fact]
		public void GetHashCode1() {
			var graphQLLocation = new GraphQLLocation();
			Assert.Equal(0, graphQLLocation.GetHashCode());
		}

		[Fact]
		public void GetHashCode2() {
			var graphQLLocation = new GraphQLLocation {
				Column = 1
			};
			Assert.Equal(1.GetHashCode(), graphQLLocation.GetHashCode());
		}

		[Fact]
		public void GetHashCode3() {
			var graphQLLocation = new GraphQLLocation {
				Line = 1
			};
			Assert.Equal(0.GetHashCode() ^ 1.GetHashCode(), graphQLLocation.GetHashCode());
		}

		[Fact]
		public void GetHashCode4() {
			var graphQLLocation = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			Assert.Equal(1.GetHashCode() ^ 2.GetHashCode(), graphQLLocation.GetHashCode());
		}

		[Fact]
		public void Inequality1() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
			Assert.False(graphQLLocation1.Equals(graphQLLocation2));
			Assert.False(graphQLLocation2.Equals(graphQLLocation1));
		}

		[Fact]
		public void Inequality2() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 2,
				Line = 1
			};
			Assert.False(graphQLLocation1.Equals(graphQLLocation2));
			Assert.False(graphQLLocation2.Equals(graphQLLocation1));
		}

		[Fact]
		public void Inequality3() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
			Assert.True(graphQLLocation1 != graphQLLocation2);
			Assert.True(graphQLLocation2 != graphQLLocation1);
		}

		[Fact]
		public void Inequality4() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 2,
				Line = 1
			};
			Assert.True(graphQLLocation1 != graphQLLocation2);
			Assert.True(graphQLLocation2 != graphQLLocation1);
		}

		[Fact]
		public void Inequality5() {
			var graphQLLocation1 = new GraphQLLocation();
			var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
			Assert.False(graphQLLocation1.Equals((object)graphQLLocation2));
			Assert.False(graphQLLocation2.Equals((object)graphQLLocation1));
		}

		[Fact]
		public void Inequality6() {
			var graphQLLocation1 = new GraphQLLocation {
				Column = 1,
				Line = 2
			};
			var graphQLLocation2 = new GraphQLLocation {
				Column = 2,
				Line = 1
			};
			Assert.False(graphQLLocation1.Equals((object)graphQLLocation2));
			Assert.False(graphQLLocation2.Equals((object)graphQLLocation1));
		}

	}

}
