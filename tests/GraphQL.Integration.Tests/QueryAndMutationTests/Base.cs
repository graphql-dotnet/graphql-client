using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Client.Tests.Common.StarWars;
using GraphQL.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests {

	public abstract class Base: IAsyncLifetime {

		protected IntegrationServerTestFixture Fixture;
		protected GraphQLHttpClient StarWarsClient;
		protected GraphQLHttpClient ChatClient;

		protected Base(IntegrationServerTestFixture fixture) {
			Fixture = fixture;
		}

		public async Task InitializeAsync() {
			await Fixture.CreateServer();
			StarWarsClient = Fixture.GetStarWarsClient();
			ChatClient = Fixture.GetChatClient();
		}

		public Task DisposeAsync() {
			ChatClient?.Dispose();
			StarWarsClient?.Dispose();
			return Task.CompletedTask;
		}

		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest($"{{ human(id: \"{id}\") {{ name }} }}");
			var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty }});

			Assert.Null(response.Errors);
			Assert.Equal(name, response.Data.Human.Name);
		}

		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void QueryWithDynamicReturnTypeTheory(int id, string name) {
			var graphQLRequest = new GraphQLRequest($"{{ human(id: \"{id}\") {{ name }} }}");

			var response = await StarWarsClient.SendQueryAsync<dynamic>(graphQLRequest);

			Assert.Null(response.Errors);
			Assert.Equal(name, response.Data.human.name.ToString());
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

			var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });

			Assert.Null(response.Errors);
			Assert.Equal(name, response.Data.Human.Name);
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

			var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });

			Assert.Null(response.Errors);
			Assert.Equal(name, response.Data.Human.Name);
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

			var mutationResponse = await StarWarsClient.SendMutationAsync(mutationRequest, () => new {
					createHuman = new {
						Id = "",
						Name = "",
						HomePlanet = ""
					}
				});

			Assert.Null(mutationResponse.Errors);
			Assert.Equal("Han Solo", mutationResponse.Data.createHuman.Name);
			Assert.Equal("Corellia", mutationResponse.Data.createHuman.HomePlanet);

			queryRequest.Variables = new {id = mutationResponse.Data.createHuman.Id};
			var queryResponse = await StarWarsClient.SendQueryAsync(queryRequest, () => new { Human = new { Name = string.Empty } });
			
			Assert.Null(queryResponse.Errors);
			Assert.Equal("Han Solo", queryResponse.Data.Human.Name);
		}

		[Fact]
		public async void PreprocessHttpRequestMessageIsCalled() {
			var callbackTester = new CallbackMonitor<HttpRequestMessage>();
			var graphQLRequest = new GraphQLHttpRequest($"{{ human(id: \"1\") {{ name }} }}") {
				PreprocessHttpRequestMessage = callbackTester.Invoke
			};

			var defaultHeaders = StarWarsClient.HttpClient.DefaultRequestHeaders;
			var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });
			callbackTester.Should().HaveBeenInvokedWithPayload().Which.Headers.Should().BeEquivalentTo(defaultHeaders);
			Assert.Null(response.Errors);
			Assert.Equal("Luke", response.Data.Human.Name);
		}

		[Fact]
		public void PostRequestCanBeCancelled() {
			var graphQLRequest = new GraphQLRequest(@"
				query Long {
					longRunning
				}");

			var chatQuery = Fixture.Server.Services.GetService<ChatQuery>();
			var cts = new CancellationTokenSource();

			var request =
				ConcurrentTaskWrapper.New(() => ChatClient.SendQueryAsync(graphQLRequest, () => new { longRunning = string.Empty }, cts.Token));

			// Test regular request
			// start request
			request.Start();
			// wait until the query has reached the server
			chatQuery.WaitingOnQueryBlocker.Wait(1000).Should().BeTrue("because the request should have reached the server by then");
			// unblock the query
			chatQuery.LongRunningQueryBlocker.Set();
			// check execution time
			request.Invoke().Result.Data.longRunning.Should().Be("finally returned");

			// reset stuff
			chatQuery.LongRunningQueryBlocker.Reset();
			request.Clear();

			// cancellation test
			request.Start();
			chatQuery.WaitingOnQueryBlocker.Wait(1000).Should().BeTrue("because the request should have reached the server by then");
			cts.Cancel();
			request.Invoking().Should().Throw<TaskCanceledException>("because the request was cancelled");

			// let the server finish its query
			chatQuery.LongRunningQueryBlocker.Set();
		}

	}
}
