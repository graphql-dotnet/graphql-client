using System;
using System.Collections.Generic;
using GraphQL.Common.Request.Builder;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Common.Tests.Request.Builder
{
	public class QueryBuilderMutationTests
	{
		private readonly ITestOutputHelper _output;

		[GraphQLType("HumanType")]
		private class Human
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
			public List<Human> Friends { get; set; }
		}

		public QueryBuilderMutationTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void BuildSimpleHumanWithNoFriends()
		{
			var expected = @"mutation Human($human: HumanType!) {
  human(input: $human) {
    name
  }
}
";

			var actual = QueryBuilder.Mutation<Human>()
				.WithParameters(new {Human = new Human()})
				.UseParameter("input", h => h.Human)
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
				QueryBuilder.Mutation<Human>()
					.Include(h => h.ToString())
					.Build();
			});

			Assert.StartsWith("Expression must indicate a property.", ex.Message);
		}

		[Fact]
		public void BuildSimpleHumanNoParameters()
		{
			var ex = Assert.Throws<ArgumentException>(() =>
			{
				QueryBuilder.Mutation<Human>()
					.Include(h => h.Name)
					.Build();
			});

			Assert.StartsWith("Parameters have not been set for mutation.", ex.Message);
		}

		[Fact]
		public void BuildSimpleHumanWithFriends()
		{
			var expected = @"mutation Human($human: HumanType!) {
  human(input: $human) {
    friends {
      name
      age
    }
    name
  }
}
";

			var actual = QueryBuilder.Mutation<Human>()
				.WithParameters(new { Human = new Human() })
				.UseParameter("input", h => h.Human)
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
			var expected = @"mutation Human($human: HumanType!) {
  human(input: $human) {
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

			var actual = QueryBuilder.Mutation<Human>()
				.WithParameters(new { Human = new Human() })
				.UseParameter("input", h => h.Human)
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
			var expected = @"mutation Human($human: HumanType!) {
  human(input: $human) {
    name
  }
}
";

			var actual = QueryBuilder.Mutation<Human>()
				.WithParameters(new { Human = new Human() })
				.UseParameter("input", h => h.Human)
				.Include(h => h.Name)
				.Build();

			_output.WriteLine($"Expected:\n{expected}");
			_output.WriteLine($"Actual:\n{actual}");
			Assert.Equal(expected, actual);
		}
	}
}
