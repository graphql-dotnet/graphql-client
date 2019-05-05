using System.Linq;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expression;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Common.Tests.Request.Expression
{
	public class QueryExpressionTests
	{
		public class Schema
		{
			public Person Person { get; set; }

			public Person Hero { get; set; }
		}

		[Fact]
		public void BuildQueryFact()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new
				{
					x.Hero.Name
				}
			});

			var query = expression.Build();
			var graphQLRequest = new GraphQLRequest(@"query   {
    hero {
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
				hero = new
				{
					schema.Hero.Name,

					friends = schema.Hero.Friends.Select(f => new
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
		public void SimpleBuildQueryFact()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new
				{
					x.Hero.Name
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
			var expression = Gql<Schema>.Query(x => new
			{
				hero = new
				{
					heroName = x.Hero.Name
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
		public void TestParameters()
		{
			var expression = Gql<Schema>.Query(x => new
			{
				hero = Gql.Field(x.Hero, h => new
				{
					h.Name
				}, new {test = default(int)})
			});


			Assert.True(expression.Root.Nodes[0].Parameters.Count == 1);
		}

		[Fact]
		public void TestQueryParameter()
		{
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				hero = Gql.Field(schema.Hero, h => new
				{
					h.Name
				}, new {args.first, offset = 10})
			}, new {first = default(int)});

			var graphQLRequest = new GraphQLRequest(@"query ($first: Int!)   {
    hero (first: $first, offset: 10) {
      name
    }
  }
");

			var query = expression.Build();
			//Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
			Assert.Equal(graphQLRequest.Query, query.Query);
		}


		[Fact]
		public void TestQueryParemeterNew()
		{
			//			var firstParameter = GraphQLParameter.Build<int>("first", true);
			var expression = Gql<Schema>.Query((schema, args) => new
			{
				hero = Gql.Field(schema.Hero, x => new
				{
					x.Name
				}, new {args.first, last = 3})
			}, new {first = default(int)});

			var graphQLRequest = new GraphQLRequest(@"query ($first: Int!)   {
    hero (first: $first, last: 3) {
      name
    }
  }
");

			var parameters = expression.Variables.Select(x => new GraphQLParameter
			{
				IsRequired = x.IsRequired,
				Value = x.Value,
				Name = x.Name,
				GraphQLType = x.GqlType
			}).ToArray();

			var query = expression.Root.ToQuery(parameters);
			//			Assert.Equal(graphQLRequest.Query, query.Replace("\r\n", "\n"));
			Assert.Equal(graphQLRequest.Query, query);
		}
	}
}
