using System.Linq;
using GraphQL.Common.Request;
using GraphQL.Common.Request.Expression;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientGetTests : BaseGraphQLClientTest {

		[Fact]
		public async void QueryGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				{
					person(personID: ""1"") {
						name
					}
				}");
			var response = await this.GraphQLClient.GetAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNameGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Person{
					person(personID: ""1"") {
						name
					}
				}

				query Planet {
					planet(planetID: ""1"") {
						name
					}
				}") {
				OperationName = "Person"
			};
			var response = await this.GraphQLClient.GetAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void VariablesGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest (@"
				query Person($personId: ID!){
					person(personID: $personId) {
						name
					}
				}") {
				Variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClient.GetAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNameVariableGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Person($personId: ID!){
					person(personID: $personId) {
						name
					}
				}

				query Planet {
					planet(planetID: ""1"") {
						name
					}
				}"){
				OperationName = "Person",
				Variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClient.GetAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationFilms()
		{
			var expression = Gql<SwapiSchema>.Query((schema, args) => new
			{
				person = Gql.Field(schema.Person, x => new
				{
					x.Name,
					FilmConnection = new
					{
						films = x.FilmConnection.Films.Select(f => new
						{
							f.Title
						})
					}
				}, new { args.personID }),

				allFilms = Gql.Field(schema.AllFilms, x => new
				{
					films = x.Films.Select(f => new
					{
						f.Title
					})
				})
			}, new { personID = default(GqlID<int>) });


			var response = await this.GraphQLClientSwapi.SendAsync(expression, new { personID = GqlID.From(1) })
				.ConfigureAwait(false);

			Assert.Equal(4, response.Data.person.FilmConnection.films.Count());

		}

	}

}
