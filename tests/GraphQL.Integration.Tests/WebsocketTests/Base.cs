using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests {
	public abstract class Base {
		protected readonly ITestOutputHelper output;
		protected readonly IGraphQLWebsocketJsonSerializer serializer;
		protected IWebHost CreateServer(int port) => WebHostHelpers.CreateServer<StartupChat>(port);

		public Base(ITestOutputHelper output, IGraphQLWebsocketJsonSerializer serializer) {
			this.output = output;
			this.serializer = serializer;
		}

		public Base(ITestOutputHelper output) {
			this.output = output;
		}
		
		[Fact]
		public async void AssertTestingHarness() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);

				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}


		[Fact]
		public async void CanSendRequestViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true, serializer);
				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}

		[Fact]
		public async void CanHandleRequestErrorViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true, serializer);
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
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				using (var tester = observable.Monitor()) {
					const string message1 = "Hello World";

					var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
					response.Data.AddMessage.Content.Should().Be(message1);
					tester.Should().HaveReceivedPayload(TimeSpan.FromSeconds(3))
						.Which.Data.MessageAdded.Content.Should().Be(message1);

					const string message2 = "lorem ipsum dolor si amet";
					response = await client.AddMessageAsync(message2).ConfigureAwait(false);
					response.Data.AddMessage.Content.Should().Be(message2);
					tester.Should().HaveReceivedPayload()
						.Which.Data.MessageAdded.Content.Should().Be(message2);

					// disposing the client should throw a TaskCanceledException on the subscription
					client.Dispose();
					tester.Should().HaveCompleted();
				}
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
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.Monitor();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message1);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message1);

				const string message2 = "How are you?";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message2);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message2);

				Debug.WriteLine("disposing subscription...");
				tester.Dispose();
				await Task.Delay(500);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating new subscription...");
				tester = observable.Monitor();
				tester.Should().HaveReceivedPayload(TimeSpan.FromSeconds(10))
					.Which.Data.MessageAdded.Content.Should().Be(message2);

				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message3);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message3);

				// disposing the client should complete the subscription
				client.Dispose();
				tester.Should().HaveCompleted();
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
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
				await client.InitializeWebsocketConnection();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable1 =
					client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);
				IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> observable2 =
					client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(SubscriptionRequest2, callbackTester2.Callback);

				Debug.WriteLine("subscribing...");
				var tester = observable1.Monitor();
				var tester2 = observable2.Monitor();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message1);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message1);
				
				var joinResponse = await client.JoinDeveloperUser().ConfigureAwait(false);
				joinResponse.Data.Join.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");

				var payload = tester2.Should().HaveReceivedPayload().Subject;
				payload.Data.UserJoined.Id.Should().Be("1", "because that's the id we sent with our mutation request");
				payload.Data.UserJoined.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");

				Debug.WriteLine("disposing subscription...");
				tester2.Dispose();

				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message3);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message3);

				// disposing the client should complete the subscription
				client.Dispose();
				tester.Should().HaveCompleted();
			}
		}


		[Fact]
		public async void CanHandleConnectionTimeout() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var server = CreateServer(port);
			var callbackTester = new CallbackTester<Exception>();

			var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
			await client.InitializeWebsocketConnection();
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Callback);

			Debug.WriteLine("subscribing...");
			var tester = observable.Monitor();
			const string message1 = "Hello World";

			var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message1);
			tester.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message1);

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
			tester.Should().HaveCompleted(TimeSpan.FromSeconds(5));
			server.Dispose();
		}


		[Fact]
		public async void CanHandleSubscriptionError() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
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
				using (var tester = observable.Monitor()) {
					tester.Should().HaveReceivedPayload()
						.Which.Errors.Should().ContainSingle();
					tester.Should().HaveCompleted();
					client.Dispose();
				}
			}
		}


		[Fact]
		public async void CanHandleQueryErrorInSubscription() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {

				var test = new GraphQLRequest("tset", new { test = "blaa" });

				var client = WebHostHelpers.GetGraphQLClient(port, serializer: serializer);
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
				using (var tester = observable.Monitor()) {
					tester.Should().HaveReceivedPayload()
						.Which.Errors.Should().ContainSingle();
					tester.Should().HaveCompleted();
					client.Dispose();
				}
			}
		}
	}
}
