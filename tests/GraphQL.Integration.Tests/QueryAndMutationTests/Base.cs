using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Client.Tests.Common.StarWars;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests {

	public abstract class Base {

		protected IGraphQLWebsocketJsonSerializer serializer;

		private TestServerSetup SetupTest(bool requestsViaWebsocket = false) => WebHostHelpers.SetupTest<StartupStarWars>(requestsViaWebsocket, serializer);

		protected Base(IGraphQLWebsocketJsonSerializer serializer) {
			this.serializer = serializer;
		}

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
			var callbackTester = new CallbackMonitor<HttpRequestMessage>();
			var graphQLRequest = new GraphQLHttpRequest($"{{ human(id: \"1\") {{ name }} }}") {
				PreprocessHttpRequestMessage = callbackTester.Invoke
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

		[Fact]
		public void PostRequestCanBeCancelled() {
			var graphQLRequest = new GraphQLRequest(@"
				query Long {
					longRunning
				}");

			using (var setup = WebHostHelpers.SetupTest<StartupChat>(false, serializer)) {
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

				Func<Task> requestTask = () => setup.Client.SendQueryAsync(graphQLRequest, () => new {longRunning = string.Empty}, cts.Token);
				Action timeMeasurement = () => requestTask.Should().Throw<TaskCanceledException>();

				timeMeasurement.ExecutionTime().Should().BeCloseTo(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(200));
			}
		}
	}
}
