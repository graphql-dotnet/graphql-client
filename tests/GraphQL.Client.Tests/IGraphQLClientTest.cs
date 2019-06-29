using GraphQL.Common.Request;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests {

	public class IGraphQLClientTest : BaseGraphQLClientTest {

		[Fact]
		public async void QueryGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				{
					repository(owner: ""graphql-dotnet"", name: ""graphql-client"") {
						name
					}
				}"
			);
			var response = await this.ServerGraphQLClient.SendQueryAsync(graphQLRequest);
			Assert.Equal("graphql-client", response.Data.repository.name.Value);
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
			var response = await this.GraphQLClientSwapi.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void VariablesGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Person($personId: ID!){
					person(personID: $personId) {
						name
					}
				}") {
				Variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClientSwapi.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

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
				}") {
				OperationName = "Person",
				Variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClientSwapi.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

	}

}
