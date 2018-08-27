using System;
using System.Net.Http;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests.Http {

	public class HttpClientExtensionsTest : BaseGraphQLClientTest {

		public GraphQLHttpClient GraphQLHttpClient => new HttpClient().AsGraphQLClient(new GraphQLHttpClientOptions {EndPoint=new Uri( "https://swapi.apis.guru/") });

		[Fact]
		public async void QueryGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest {
				query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			var response = await this.GraphQLHttpClient.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNameGetAsyncFact() {
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
			var response = await this.GraphQLHttpClient.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void VariablesGetAsyncFact() {
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
			var response = await this.GraphQLHttpClient.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

		[Fact]
		public async void OperationNameVariableGetAsyncFact() {
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
			var response = await this.GraphQLHttpClient.SendQueryAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

	}

}
