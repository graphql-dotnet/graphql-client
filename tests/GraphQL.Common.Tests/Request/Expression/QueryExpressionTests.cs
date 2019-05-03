using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expression;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Common.Tests.Request.Expression
{
	public class QueryExpressionTests
	{
		
		[Fact]
		public void BuildQueryFact()
		{
			var expression = GraphQLExpression<Person>.Build(x => new
			{
				hero = new
				{
					x.Name,
				}
			});

			var query = expression.Root.ToString();
			var graphQLRequest = new GraphQLRequest(@" {
  hero {
    name
  }
}
");

			Assert.Equal(graphQLRequest.Query, query);
		}



		[Fact]
		public void SimpleBuildQueryFact()
		{
			var expression = GraphQLExpression<Person>.Build(x => new
			{
				hero = new
				{
					x.Name,
				}
			});

			var request = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero {
      name
    }
  }
");

			Assert.Equal(graphQLRequest, request);
		}




		[Fact] public void SimpleBuildQueryWithAliasFact()
		{
			var expression = GraphQLExpression<Person>.Build(x => new
			{
				hero = new
				{
					heroName = x.Name,
				}
			});

			var request = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero {
      heroName: name
    }
  }
");

			Assert.Equal(graphQLRequest, request);
		}

		[Fact]
		public void BuildWithSubLinq()
		{
			var expression = GraphQLExpression<Person>.Build(x => new
			{
				hero = new
				{
					x.Name,

					friends = x.Friends.Select(f=>new
					{
						f.Name
					})
				}
			});

			
			var request = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero {
      name
      friends {
        name
      }
    }
  }
");

			Assert.Equal(graphQLRequest, request);
		}


	}
}
