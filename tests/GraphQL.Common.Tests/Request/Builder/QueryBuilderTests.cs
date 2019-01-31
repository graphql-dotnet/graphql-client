using System;
using System.Collections.Generic;
using GraphQL.Common.Request.Builder;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Common.Tests.Request.Builder
{
	public class QueryBuilderTests
	{
		private readonly ITestOutputHelper _output;

		private class Human
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public List<Human> Friends { get; set; }
		}

		public QueryBuilderTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void BuildSimpleHumanWithNoFriends()
		{
			var expected = @"query Human {
  human {
    name
  }
}
";

			var actual = QueryBuilder.New<Human>()
				.Include(h => h.Name)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanUsingNonProperty()
		{
			var ex = Assert.Throws<ArgumentException>(() =>
			{
				QueryBuilder.New<Human>()
					.Include(h => h.ToString())
					.Build();
			});

			Assert.StartsWith("Expression must indicate a property.", ex.Message);
		}

		[Fact]
		public void BuildSimpleHumanWithFriends()
		{
			var expected = @"query Human {
  human {
    friends {
      name
      age
    }
    name
  }
}
";

			var actual = QueryBuilder.New<Human>()
				.Include(h => h.Friends)
				.ThenInclude(f => f.Name)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Age)
				.Include(h => h.Name)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanWithFriendsOfFriends()
		{
			var expected = @"query Human {
  human {
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
}
";

			var actual = QueryBuilder.New<Human>()
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

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanWithId()
		{
			var expected = @"query Human($id: Int!) {
  human(id: $id) {
    name
  }
}
";

			var actual = QueryBuilder.New<Human>()
				.WithParameters(new {Id = 1})
				.UseParameter("id", p => p.Id)
				.Include(h => h.Name)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanWithFriendId()
		{
			var expected = @"query Human($id: Int!) {
  human {
    name
    friends(id: $id) {
      name
    }
  }
}
";

			var actual = QueryBuilder.New<Human>()
				.WithParameters(new {Id = 1})
				.Include(h => h.Name)
				.Include(h => h.Friends)
				.UseParameter("id", p => p.Id)
				.ThenInclude(f => f.Name)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void BuildSimpleHumanWithFriendIdGetNameAndAge()
		{
			var expected = @"query Human($id: Int!) {
  human {
    name
    friends(id: $id) {
      name
      age
    }
  }
}
";

			var actual = QueryBuilder.New<Human>()
				.WithParameters(new {Id = 1})
				.Include(h => h.Name)
				.Include(h => h.Friends)
				.UseParameter("id", p => p.Id)
				.ThenInclude(f => f.Name)
				.Include(h => h.Friends)
				.ThenInclude(f => f.Age)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}
	}
}
