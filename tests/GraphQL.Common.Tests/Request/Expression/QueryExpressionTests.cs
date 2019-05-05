using System;
using System.Linq;
using System.Net;
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
			var expression = Gql<Schema>.Query(x => new
			{
				MainHero = new
				{
					x.MainHero.Name
				}
			});

			var query = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    mainHero {
      name
    }
  }
");


			Assert.Equal(graphQLRequest.Query, query.Query); //.Replace("\r\n", "\n"));
		}

		[Fact]
		public void BuildQueryUsingSubField()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				MainHero = Gql.Field(x.MainHero, h => new
				{
					h.Name
				})
			});

			var query = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    mainHero {
      name
    }
  }
");

			Assert.Equal(graphQLRequest.Query, query.Query); //.Replace("\r\n", "\n"));
		}

		[Fact]
		public void BuildQueryUsingAlias()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new 
				{
					x.MainHero.Name
				}
			});

			var query = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero: mainHero {
      name
    }
  }
");

			Assert.Equal(graphQLRequest.Query, query.Query); //.Replace("\r\n", "\n"));
		}
		[Fact]
		public void BuildWithSubLinq()
		{
			var expression = Gql<Schema>.Query(schema => new
			{
				MainHero = new
				{
					schema.MainHero.Name,

					friends = schema.MainHero.Friends.Select(f => new
					{
						f.Name
					})
				}
			});


			var request = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    mainHero {
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
		public void SimpleBuildQueryWithAliasFact()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new
				{
					heroName = x.MainHero.Name
				}
			});

			var request = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero: mainHero {
      heroName: name
    }
  }
");

			Assert.Equal(graphQLRequest, request);
		}


		[Fact]
		public void TestParameters()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = Gql.Field(x.MainHero, h => new
				{
					h.Name
				}, new { test = default(int) })
			});


			Assert.True(expression.Root.Nodes[0].Parameters.Count == 1);
		}

		[Fact]
		public void TestQueryParameter()
		{
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				heroes = Gql.Field(schema.Heroes, h => new
				{
					h.Name
				}, new { args.first, offset = 10 })
			}, new { first = default(int) });

			var graphQLRequest = new GraphQLRequest(@"query ($first: Int!)   {
    heroes (first: $first, offset: 10) {
      name
    }
  }
");

			var query = expression.Build();
			//Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
			Assert.Equal(graphQLRequest.Query, query.Query);
		}


		[Fact]
		public void TestQueryParameterNew()
		{
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				hero = Gql.Field(schema.MainHero, x => new
				{
					x.Name
				}, new { args.first, last = 3 })
			}, new { first = default(int) });

			var graphQLRequest = new GraphQLRequest(@"query ($first: Int!)   {
    hero: mainHero (first: $first, last: 3) {
      name
    }
  }
");

			var request = expression.Build();
			//			Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
			Assert.Equal(graphQLRequest.Query, request.Query);
		}
	}
}
