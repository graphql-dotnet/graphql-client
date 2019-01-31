using System;
using System.Collections.Generic;
using GraphQL.Common.Request.Builder;
using Xunit;

namespace GraphQL.Common.Tests.Request.Builder
{
	public class QueryBuilderTests
	{
		private class Human
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public List<Human> Friends { get; set; }
		}

		[Fact]
		public void BuildSimpleHumanWithNoFriends()
		{
			var expected = @"human {
  name
}
";

			var actual = new QueryBuilder<Human>()
				.Include(h => h.Name)
				.Build();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanUsingNonProperty()
		{
			var ex = Assert.Throws<ArgumentException>(() =>
			{
				new QueryBuilder<Human>()
					.Include(h => h.ToString())
					.Build();
			});

			Assert.StartsWith("Expression must indicate a property.", ex.Message);
		}

		[Fact]
		public void BuildSimpleHumanWithFriends()
		{
			var expected = @"human {
  friends {
    name
    age
  }
  name
}
";

			var actual = new QueryBuilder<Human>()
				.Include(h => h.Friends)
				.ThenInclude(f => f.Name)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Age)
				.Include(h => h.Name)
				.Build();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanWithFriendsOfFriends()
		{
			var expected = @"human {
  friends {
    name
    age
    friends {
      name
      age
    }
  }
  name
}
";

			var actual = new QueryBuilder<Human>()
				.Include(h => h.Friends)
				.ThenInclude(f => f.Name)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Age)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Friends)
				.ThenInclude(f => f.Name)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Friends)
				.ThenInclude(f => f.Age)
				.Include(h => h.Name)
				.Build();

			Assert.Equal(expected, actual);
		}
	}
}
