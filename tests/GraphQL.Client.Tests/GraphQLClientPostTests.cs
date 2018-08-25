using System.Net.Http.Headers;
using GraphQL.Common.Request;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientPostTests : BaseGraphQLClientTest {

		[Fact]
		public async void QueryPostAsyncFact() {
			var graphQLRequest = new GraphQLRequest {
				query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			var response = await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void QueryPostAsyncWithoutUtf8EncodingFact()
		{
			var graphQLRequest = new GraphQLRequest
			{
				query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			this.GraphQLClient.Options.MediaType = MediaTypeHeaderValue.Parse("application/json");
			var response = await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNamePostAsyncFact() {
			var graphQLRequest = new GraphQLRequest {
				query = @"
				query Person{
					person(personID: ""1"") {
						name
					}
				}

				query Planet {
					planet(planetID: ""1"") {
						name
					}
				}",
				operationName = "Person"
			};
			var response = await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void VariablesPostAsyncFact() {
			var graphQLRequest = new GraphQLRequest {
				query = @"
				query Person($personId: ID!){
					person(personID: $personId) {
						name
					}
				}",
				variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNameVariablePostAsyncFact() {
			var graphQLRequest = new GraphQLRequest {
				query = @"
				query Person($personId: ID!){
					person(personID: $personId) {
						name
					}
				}

				query Planet {
					planet(planetID: ""1"") {
						name
					}
				}",
				operationName = "Person",
				variables = new {
					personId = "1"
				}
			};
			var response = await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

	}

}
