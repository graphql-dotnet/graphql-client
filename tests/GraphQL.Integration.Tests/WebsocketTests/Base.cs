using System;
using System.Collections.Concurrent;
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
		protected readonly ITestOutputHelper Output;
		protected readonly IGraphQLWebsocketJsonSerializer Serializer;
		protected IWebHost CreateServer(int port) => WebHostHelpers.CreateServer<StartupChat>(port);

		protected Base(ITestOutputHelper output, IGraphQLWebsocketJsonSerializer serializer) {
			this.Output = output;
			this.Serializer = serializer;
		}
		
		[Fact]
		public async void AssertTestingHarness() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);

				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}

		[Fact]
		public async void CanSendRequestViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true, Serializer);
				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
			}
		}

		[Fact]
		public void WebsocketRequestCanBeCancelled() {
			var graphQLRequest = new GraphQLRequest(@"
				query Long {
					longRunning
				}");

			using (var setup = WebHostHelpers.SetupTest<StartupChat>(true, Serializer)) {
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

				Func<Task> requestTask = () => setup.Client.SendQueryAsync(graphQLRequest, () => new { longRunning = string.Empty }, cts.Token);
				Action timeMeasurement = () => requestTask.Should().Throw<TaskCanceledException>();

				timeMeasurement.ExecutionTime().Should().BeCloseTo(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(200));
			}
		}

		[Fact]
		public async void CanHandleRequestErrorViaWebsocket() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, true, Serializer);
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
			using (CreateServer(port)){
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
				var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();
				await client.InitializeWebsocketConnection();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();

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
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
				var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();

				Debug.WriteLine("creating subscription stream");
				var observable = client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.Monitor();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();

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
				tester.Dispose(); // does not close the websocket connection

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
			var callbackTester = new CallbackMonitor<Exception>();
			var callbackTester2 = new CallbackMonitor<Exception>();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
				var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();
				await client.InitializeWebsocketConnection();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable1 =
					client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Invoke);
				IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> observable2 =
					client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(SubscriptionRequest2, callbackTester2.Invoke);

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
			var errorMonitor = new CallbackMonitor<Exception>();
			var reconnectBlocker = new ManualResetEventSlim(false);

			var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
			var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();
			// configure back-off strategy to allow it to be controlled from within the unit test
			client.Options.BackOffStrategy = i => {
				reconnectBlocker.Wait();
				return TimeSpan.Zero;
			};

			var websocketStates = new ConcurrentQueue<GraphQLWebsocketConnectionState>();

			using (client.WebsocketConnectionState.Subscribe(websocketStates.Enqueue)) {
				websocketStates.Should().ContainSingle(state => state == GraphQLWebsocketConnectionState.Disconnected);

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse<MessageAddedSubscriptionResult>> observable =
					client.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest,
						errorMonitor.Invoke);

				Debug.WriteLine("subscribing...");
				var tester = observable.Monitor();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();

				websocketStates.Should().ContainInOrder(
					GraphQLWebsocketConnectionState.Disconnected,
					GraphQLWebsocketConnectionState.Connecting,
					GraphQLWebsocketConnectionState.Connected);
				// clear the collection so the next tests on the collection work as expected
				websocketStates.Clear();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message1);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message1);

				Debug.WriteLine("stopping web host...");
				await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
				server.Dispose();
				Debug.WriteLine("web host stopped...");

				errorMonitor.Should().HaveBeenInvokedWithPayload(TimeSpan.FromSeconds(10))
					.Which.Should().BeOfType<WebSocketException>();
				websocketStates.Should().Contain(GraphQLWebsocketConnectionState.Disconnected);

				server = CreateServer(port);
				reconnectBlocker.Set();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();
				websocketStates.Should().ContainInOrder(
					GraphQLWebsocketConnectionState.Disconnected,
					GraphQLWebsocketConnectionState.Connecting,
					GraphQLWebsocketConnectionState.Connected);

				// disposing the client should complete the subscription
				client.Dispose();
				tester.Should().HaveCompleted(TimeSpan.FromSeconds(5));
				server.Dispose();
			}
		}


		[Fact]
		public async void CanHandleSubscriptionError() {
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port)) {
				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
				var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();
				await client.InitializeWebsocketConnection();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();
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
					tester.Should().HaveReceivedPayload(TimeSpan.FromSeconds(3))
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

				var client = WebHostHelpers.GetGraphQLClient(port, serializer: Serializer);
				var callbackMonitor = client.ConfigureMonitorForOnWebsocketConnected();
				await client.InitializeWebsocketConnection();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();
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
