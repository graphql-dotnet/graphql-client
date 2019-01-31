using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Common.Request.Builder;
using Xunit;

namespace GraphQL.Common.Tests.Request.Builder
{
	public class QueryBuilderTests
	{
		private class Human
		{
			public string Name { get; set; }
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
	}
}
