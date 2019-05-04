using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
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
			var expression = GqlExp<Person>.Build(x => new
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

			Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
		}



		[Fact]
		public void SimpleBuildQueryFact()
		{
			var expression = GqlExp<Person>.Build(x => new
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




		[Fact]
		public void SimpleBuildQueryWithAliasFact()
		{
			var expression = GqlExp<Person>.Build(x => new
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
			var expression = GqlExp<Person>.Build(x => new
			{
				hero = new
				{
					x.Name,

					friends = x.Friends.Select(f => new
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


		[Fact]
		public void TestParameters()
		{
			var parameter = GraphQLParameter.Build("test");
			var expression = GqlExp<Person>.Build(x => new
			{
				hero = x.WithParameters(t => new
				{
					t.Name
				}, parameter)
			});


			Assert.True(expression.Root.Nodes[0].Parameters.Length == 1);
			Assert.Equal(parameter, expression.Root.Nodes[0].Parameters[0]);
		}

		[Fact]
		public void TestQueryParameter()
		{
			var firstParameter = GraphQLParameter.Build<int>("first", true);
			var expression = GqlExp<Person>.Build(new[] { firstParameter }, x => new
			{
				hero = GqlExp.Params(new[] { firstParameter.ToArgument() }, x, t => new
				{
					t.Name
				})
			});

			var graphQLRequest = new GraphQLRequest(@"query ($first: Int!)   {
    hero (first: $first) {
      name
    }
  }
");

			var query = expression.Root.ToQuery(expression.Parameters);
			Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
		}
	}
}
