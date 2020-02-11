using System.Net.Http;
using System.Text.Json;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.StarWars;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GraphQL.Integration.Tests {
	public class QueryAndMutationTests {

		private static TestServerSetup SetupTest(bool requestsViaWebsocket = false) => WebHostHelpers.SetupTest<StartupStarWars>(requestsViaWebsocket);
		
		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest($"{{ human(id: \"{id}\") {{ name }} }}");

			using (var setup = SetupTest()) {
				var response = await setup.Client.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty }})
					.ConfigureAwait(false);

				Assert.Null(response.Errors);
				Assert.Equal(name, response.Data.Human.Name);
			}
		}

		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryWithDynamicReturnTypeTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest($"{{ human(id: \"{id}\") {{ name }} }}");

			using (var setup = SetupTest()) {
				var response = await setup.Client.SendQueryAsync<dynamic>(graphQLRequest)
					.ConfigureAwait(false);

				Assert.Null(response.Errors);
				Assert.Equal(name, response.Data.human.name.ToString());
			}
		}

		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryWitVarsTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest(@"
				query Human($id: String!){
					human(id: $id) {
						name
					}
				}",
				new {id = id.ToString()});

			using (var setup = SetupTest()) {
				var response = await setup.Client.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } })
					.ConfigureAwait(false);

				Assert.Null(response.Errors);
				Assert.Equal(name, response.Data.Human.Name);
			}
		}

		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryWitVarsAndOperationNameTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest(@"
				query Human($id: String!){
					human(id: $id) {
						name
					}
				}

				query Droid($id: String!) {
				  droid(id: $id) {
				    name
				  }
				}",
				new { id = id.ToString() },
				"Human");

			using (var setup = SetupTest()) {
				var response = await setup.Client.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } })
					.ConfigureAwait(false);

				Assert.Null(response.Errors);
				Assert.Equal(name, response.Data.Human.Name);
			}
		}

		[Fact]
		public async void SendMutationFact() {
			var mutationRequest = new GraphQLRequest(@"
				mutation CreateHuman($human: HumanInput!) {
				  createHuman(human: $human) {
				    id
				    name
				    homePlanet
				  }
				}",
				new { human = new { name = "Han Solo", homePlanet = "Corellia"}});

			var queryRequest = new GraphQLRequest(@"
				query Human($id: String!){
					human(id: $id) {
						name
					}
				}");

			using (var setup = SetupTest()) {
				var mutationResponse = await setup.Client.SendMutationAsync(mutationRequest, () => new {
						createHuman = new {
							Id = "",
							Name = "",
							HomePlanet = ""
						}
					})
					.ConfigureAwait(false);

				Assert.Null(mutationResponse.Errors);
				Assert.Equal("Han Solo", mutationResponse.Data.createHuman.Name);
				Assert.Equal("Corellia", mutationResponse.Data.createHuman.HomePlanet);

				queryRequest.Variables = new {id = mutationResponse.Data.createHuman.Id};
				var queryResponse = await setup.Client.SendQueryAsync(queryRequest, () => new { Human = new { Name = string.Empty } })
					.ConfigureAwait(false);
				
				Assert.Null(queryResponse.Errors);
				Assert.Equal("Han Solo", queryResponse.Data.Human.Name);
			}
		}

		[Fact]
		public async void PreprocessHttpRequestMessageIsCalled() {
			var callbackTester = new CallbackTester<HttpRequestMessage>();
			var graphQLRequest = new GraphQLHttpRequest($"{{ human(id: \"1\") {{ name }} }}") {
				PreprocessHttpRequestMessage = callbackTester.Callback
			};

			using (var setup = SetupTest()) {
				var defaultHeaders = setup.Client.HttpClient.DefaultRequestHeaders;
				var response = await setup.Client.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } })
					.ConfigureAwait(false);
				callbackTester.CallbackShouldHaveBeenInvoked(message => {
					Assert.Equal(defaultHeaders, message.Headers);
				});
				Assert.Null(response.Errors);
				Assert.Equal("Luke", response.Data.Human.Name);
			}
		}
	}
}
