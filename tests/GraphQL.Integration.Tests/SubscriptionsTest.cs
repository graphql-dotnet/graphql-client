using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GraphQL.Integration.Tests
{
	public class SubscriptionsTest
	{
		public static IWebHost CreateServer(int port)
		{
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddInMemoryCollection();
			var config = configBuilder.Build();
			config["server.urls"] = $"http://localhost:{port}";

			var host = new WebHostBuilder()
				.ConfigureLogging((ctx, logging) =>
				{
					logging.AddDebug();
				})
				.UseConfiguration(config)
				.UseKestrel()
				.UseStartup<IntegrationTestServer.Startup>()
				.Build();

			host.Start();

			return host;
		}

		private readonly IWebHost _server;

		public SubscriptionsTest()
		{
		}

		private GraphQLHttpClient GetGraphQLClient(int port, bool requestsViaWebsocket = false)
			=> new GraphQLHttpClient(new GraphQLHttpClientOptions
			{
				EndPoint = new Uri($"http://localhost:{port}/graphql"),
				UseWebSocketForQueriesAndMutations = requestsViaWebsocket
			});


		[Fact]
		public async void AssertTestingHarness()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, (string) response.Data.addMessage.content);
			}
		}

		[Fact]
		public async void CanSendRequestViaWebsocket()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port, true);
				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, (string)response.Data.addMessage.content);
			}
		}

		[Fact]
		public async void CanHandleRequestErrorViaWebsocket()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port, true);
				const string message = "some random testing message";
				var response = await client.SendQueryAsync("this query is formatted quite badly").ConfigureAwait(false);

				Assert.Single(response.Errors);
			}
		}

		private const string SubscriptionQuery = @"
			subscription {
			  messageAdded{
			    content
			  }
			}";

		private readonly GraphQLRequest SubscriptionRequest = GraphQLRequest.New(SubscriptionQuery);

		[Fact]
		public async void CanCreateObservableSubscription()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				const string message1 = "Hello World";

				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string) response.Data.addMessage.content);

				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, gqlResponse.Data.MessageAdded.Content);
				});

				const string message2 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, gqlResponse.Data.MessageAdded.Content);
				});

				// disposing the client should throw a TaskCanceledException on the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		public class MessageAddedSubscriptionResult {
			public MessageAddedContent MessageAdded { get; set; }

			public class MessageAddedContent {
				public string Content { get; set; }
			}
		}

		[Fact]
		public async void CanReconnectWithSameObservable()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string) gqlResponse.Data.MessageAdded.Content);
				});

				const string message2 = "How are you?";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string) gqlResponse.Data.MessageAdded.Content);
				});

				Debug.WriteLine("disposing subscription...");
				tester.Dispose();

				Debug.WriteLine("creating new subscription...");
				tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(
					gqlResponse => { Assert.Equal(message2, (string) gqlResponse.Data.MessageAdded.Content); },
					TimeSpan.FromSeconds(10));
				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message3, (string) gqlResponse.Data.MessageAdded.Content);
				});

				// disposing the client should complete the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		private const string SubscriptionQuery2 = @"
			subscription {
			  userJoined{
				displayName
				id
			  }
			}";

		public class UserJoinedSubscriptionResult {
			public UserJoinedContent UserJoined { get; set; }

			public class UserJoinedContent {
				public string DisplayName { get; set; }
				public string Id { get; set; }
			}

		}

		private readonly GraphQLRequest SubscriptionRequest2 = GraphQLRequest.New(SubscriptionQuery2);

		[Fact]
		public async void CanConnectTwoSubscriptionsSimultaneously()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var callbackTester = new CallbackTester<Exception>();
			var callbackTester2 = new CallbackTester<Exception>();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable1 = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);
				IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> observable2 = client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(SubscriptionRequest2, callbackTester2.Callback);

				Debug.WriteLine("subscribing...");
				var tester = observable1.SubscribeTester();
				var tester2 = observable2.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string)gqlResponse.Data.MessageAdded.Content);
				});

				await Task.Delay(500); // ToDo: can be removed after https://github.com/graphql-dotnet/server/pull/199 was merged and released

				response = await client.JoinDeveloperUser().ConfigureAwait(false);
				Assert.Equal("developer", (string)response.Data.join.displayName.Value);

				tester2.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal("1", (string)gqlResponse.Data.UserJoined.Id);
					Assert.Equal("developer", (string)gqlResponse.Data.UserJoined.DisplayName);
				});
				
				Debug.WriteLine("disposing subscription...");
				tester2.Dispose();

				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message3, (string)gqlResponse.Data.MessageAdded.Content);
				});

				// disposing the client should complete the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		[Fact]
		public async void CanHandleConnectionTimeout()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var server = CreateServer(port);
			var callbackTester = new CallbackTester<Exception>();

			var client = GetGraphQLClient(port);
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);

			Debug.WriteLine("subscribing...");
			var tester = observable.SubscribeTester();
			const string message1 = "Hello World";

			var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
			Assert.Equal(message1, (string)response.Data.addMessage.content);

			tester.ShouldHaveReceivedUpdate(gqlResponse =>
			{
				Assert.Equal(message1, (string)gqlResponse.Data.MessageAdded.Content);
			});

			Debug.WriteLine("stopping web host...");
			await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
			Debug.WriteLine("web host stopped...");

			callbackTester.CallbackShouldHaveBeenInvoked(exception =>
			{
				Assert.IsType<WebSocketException>(exception);
			}, TimeSpan.FromSeconds(10));

			try
			{
				server.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			// disposing the client should complete the subscription
			client.Dispose();
			tester.ShouldHaveCompleted(TimeSpan.FromSeconds(5));
			server.Dispose();
		}
		
		[Fact]
		public async void CanHandleSubscriptionError()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<object>> observable = client.CreateSubscriptionStream<object>(
					GraphQLRequest.New(@"
						subscription {
						  failImmediately {
						    content
						  }
						}")
					);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Single(gqlResponse.Errors);
				});
				tester.ShouldHaveCompleted();

				client.Dispose();
			}
		}

		[Fact]
		public async void CanHandleQueryErrorInSubscription()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{

				var test = GraphQLRequest.New("tset", new { test = "blaa"});

				var client = GetGraphQLClient(port);
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<object>> observable = client.CreateSubscriptionStream<object>(
					GraphQLRequest.New(@"
						subscription {
						  fieldDoesNotExist {
						    content
						  }
						}")
				);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Single(gqlResponse.Errors);
				});
				tester.ShouldHaveCompleted();

				client.Dispose();
			}
		}
	}
}
