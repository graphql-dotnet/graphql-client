using System.Linq.Expressions;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expressions;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests
{
	public class GraphQlClientExpressionTests : BaseGraphQLClientTest
	{
		[Fact]
		public async void QueryGetAsyncFact()
		{
			var request = Gql<SwapiSchema>.Query(schema => new
			{
				person = Gql.Field(schema.Person, p => new
				{
					p.Name
				}, new { personID = "1" })
			});
			var response = await this.GraphQLClient.SendAsync(request).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.Name);
		}

		[Fact]
		public async void OperationNameGetAsyncFact()
		{
			var request = Gql<SwapiSchema>.Query(schema => new
			{
				person = Gql.Field(schema.Person, p => new
				{
					name = p.Name
				}, new { personID = "1" })
			}, "Person");
			var response = await this.GraphQLClient.SendAsync(request).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name);
		}

		[Fact]
		public async void VariablesGetAsyncFact()
		{
			var request = Gql<SwapiSchema>.Query((schema, args) => new
			{
				person = Gql.Field(schema.Person, p => new
				{
					name = p.Name
				}, new { args.personID })
			}, new { personID = default (GqlID<string>) }, "Person");

			var response = await this.GraphQLClient.SendAsync(request, new {personID = GqlID.From("1")}).ConfigureAwait(false);
			Assert.Equal("Luke Skywalker", response.Data.person.name);
		}
	}
}
