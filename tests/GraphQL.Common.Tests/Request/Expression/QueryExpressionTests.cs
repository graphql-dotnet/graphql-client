using System;
using System.Linq;
using System.Net;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expressions;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Common.Tests.Request.Expression
{
	public class QueryExpressionTests
	{
		
		[Fact]
		public void SimpleQuery()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				MainHero = new
				{
					x.MainHero.Name
				}
			});

			var graphQLRequest = @"query { mainHero { name } }";

			Assert.Equal(graphQLRequest, expression.Query);
		}

		[Fact]
		public void SimpleQueryWithField()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				MainHero = Gql.Field(x.MainHero, h => new
				{
					h.Name
				})
			});

			var graphQLRequest = @"query { mainHero { name } }";

			Assert.Equal(graphQLRequest, expression.Query);
		}

		[Fact]
		public void QueryUsingAlias()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new 
				{
					x.MainHero.Name,
				}
			});

			var graphQLRequest = @"query { hero: mainHero { name } }";

			Assert.Equal(graphQLRequest, expression.Query); //.Replace("\r\n", "\n"));
		}

		[Fact]
		public void QueryWithSubLinq()
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


			var request = @"query { mainHero { name friends { name } } }";

			Assert.Equal(request, expression.Query);
		}

		[Fact]
		public void QueryWithSubLinq2()
		{
			var expression = Gql<Schema>.Query(schema => new
			{
				MainHero = new
				{
					schema.MainHero.Name,

					friends = schema.MainHero.Friends.Select(f => new
					{
						f.Name
					}),
				}
			});


			var request = @"query { mainHero { name friends { name } } }";
			Assert.Equal(request, expression.Query);
		}


		[Fact]
		public void SimpleQueryWithAliasFact()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new
				{
					heroName = x.MainHero.Name
				}
			});

			var graphQLRequest = @"query { hero: mainHero { heroName: name } }";

			Assert.Equal(graphQLRequest, expression.Query);
		}


		[Fact]
		public void QueryParameters()
		{
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				heroes = Gql.Field(schema.Heroes, h => new
				{
					h.Name
				}, new { args.first, offset = 10 })
			}, new { first = default(int) });

			var graphQLRequest = @"query ($first: Int!) { heroes (first: $first, offset: 10) { name } }";

			Assert.Equal(graphQLRequest, expression.Query);
		}


		[Fact]
		public void TestQueryParameterNew2()
		{
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				hero = Gql.Field(schema.MainHero, x => new
				{
					x.Name
				}, new { args.first, last = 3 })
			}, new { first = default(int) });

			var graphQLRequest = @"query ($first: Int!) { hero: mainHero (first: $first, last: 3) { name } }";

			Assert.Equal(graphQLRequest, expression.Query);
		}



	}
}
