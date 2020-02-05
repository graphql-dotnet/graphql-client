using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Integration.Tests.Extensions;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests {
	public class SubscriptionsTest {
		private readonly ITestOutputHelper output;

		private static IWebHost CreateServer(int port) => WebHostHelpers.CreateServer<StartupChat>(port);

		private static TimeSpan WaitForConnectionDelay = TimeSpan.FromMilliseconds(200);

		public SubscriptionsTest(ITestOutputHelper output) {
			this.output = output;
		}
		
		[Fact]
		public async void AssertTestingHarness() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port);

				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}

		[Fact]
		public async void CanSendRequestViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true);
				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}

		[Fact]
		public async void CanHandleRequestErrorViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true);
				var response = await client.SendQueryAsync<object>("this query is formatted quite badly").ConfigureAwait(false);

				Assert.Single(response.Errors);
			}
		}

		private const string SubscriptionQuery = @"
			subscription {
			  messageAdded{
			    content
			  }
			}";

		private readonly GraphQLRequest SubscriptionRequest = new GraphQLRequest(SubscriptionQuery);

		[Fact]
		public async void CanCreateObservableSubscription() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				const string message1 = "Hello World";

				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, response.Data.AddMessage.Content);

				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message1, gqlResponse.Data.MessageAdded.Content);
				});

				const string message2 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
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
		public async void CanReconnectWithSameObservable() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message1, gqlResponse.Data.MessageAdded.Content);
				});

				const string message2 = "How are you?";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message2, gqlResponse.Data.MessageAdded.Content);
				});

				Debug.WriteLine("disposing subscription...");
				tester.Dispose();
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating new subscription...");
				tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(
					gqlResponse => { Assert.Equal(message2, gqlResponse.Data.MessageAdded.Content); },
					TimeSpan.FromSeconds(10));
				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message3, gqlResponse.Data.MessageAdded.Content);
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

		private readonly GraphQLRequest SubscriptionRequest2 = new GraphQLRequest(SubscriptionQuery2);

		[Fact]
		public async void CanConnectTwoSubscriptionsSimultaneously() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var callbackTester = new CallbackTester<Exception>();
			var callbackTester2 = new CallbackTester<Exception>();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable1 = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);
				IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> observable2 = client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(SubscriptionRequest2, callbackTester2.Callback);

				Debug.WriteLine("subscribing...");
				var tester = observable1.SubscribeTester();
				var tester2 = observable2.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message1, gqlResponse.Data.MessageAdded.Content);
				});

				await Task.Delay(500); // ToDo: can be removed after https://github.com/graphql-dotnet/server/pull/199 was merged and released

				var joinResponse = await client.JoinDeveloperUser().ConfigureAwait(false);
				Assert.Equal("developer", joinResponse.Data.Join.DisplayName);

				tester2.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal("1", gqlResponse.Data.UserJoined.Id);
					Assert.Equal("developer", gqlResponse.Data.UserJoined.DisplayName);
				});

				Debug.WriteLine("disposing subscription...");
				tester2.Dispose();

				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, response.Data.AddMessage.Content);
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Equal(message3, gqlResponse.Data.MessageAdded.Content);
				});

				// disposing the client should complete the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		[Fact]
		public async void CanHandleConnectionTimeout() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var server = CreateServer(port);
			var callbackTester = new CallbackTester<Exception>();

			var client = WebHostHelpers.GetGraphQLClient(port);
			await client.InitializeWebsocketConnection();
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);

			Debug.WriteLine("subscribing...");
			var tester = observable.SubscribeTester();
			const string message1 = "Hello World";

			var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
			Assert.Equal(message1, response.Data.AddMessage.Content);

			tester.ShouldHaveReceivedUpdate(gqlResponse => {
				Assert.Equal(message1, gqlResponse.Data.MessageAdded.Content);
			});

			Debug.WriteLine("stopping web host...");
			await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
			Debug.WriteLine("web host stopped...");

			callbackTester.CallbackShouldHaveBeenInvoked(exception => {
				Assert.IsType<WebSocketException>(exception);
			}, TimeSpan.FromSeconds(10));

			try {
				server.Start();
			}
			catch (Exception e) {
				output.WriteLine($"failed to restart server: {e}");
			}

			// disposing the client should complete the subscription
			client.Dispose();
			tester.ShouldHaveCompleted(TimeSpan.FromSeconds(5));
			server.Dispose();
		}

		[Fact]
		public async void CanHandleSubscriptionError() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port);
				await client.InitializeWebsocketConnection();
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<object>> observable = client.CreateSubscriptionStream<object>(
					new GraphQLRequest(@"
						subscription {
						  failImmediately {
						    content
						  }
						}")
					);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Single(gqlResponse.Errors);
				});
				tester.ShouldHaveCompleted();

				client.Dispose();
			}
		}

		[Fact]
		public async void CanHandleQueryErrorInSubscription() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {

				var test = new GraphQLRequest("tset", new { test = "blaa" });

				var client = WebHostHelpers.GetGraphQLClient(port);
				await client.InitializeWebsocketConnection();
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<object>> observable = client.CreateSubscriptionStream<object>(
					new GraphQLRequest(@"
						subscription {
						  fieldDoesNotExist {
						    content
						  }
						}")
				);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(gqlResponse => {
					Assert.Single(gqlResponse.Errors);
				});
				tester.ShouldHaveCompleted();

				client.Dispose();
			}
		}
	}
}
