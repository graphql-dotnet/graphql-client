using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests {
	public abstract class Base: IAsyncLifetime {
		protected readonly ITestOutputHelper Output;
		protected readonly IntegrationServerTestFixture Fixture;
		protected GraphQLHttpClient ChatClient;
		
		protected Base(ITestOutputHelper output, IntegrationServerTestFixture fixture) {
			this.Output = output;
			this.Fixture = fixture;
		}

		public async Task InitializeAsync() {
			Fixture.Server.Services.GetService<Chat>().Reset();
			ChatClient = Fixture.GetChatClient(true);
			Output.WriteLine($"ChatClient: {ChatClient.GetHashCode()}");
		}

		public Task DisposeAsync() {
			ChatClient?.Dispose();
			return Task.CompletedTask;
		}

		[Fact]
		public async void CanSendRequestViaWebsocket() {
			await ChatClient.InitializeWebsocketConnection();
			const string message = "some random testing message";
			var response = await ChatClient.AddMessageAsync(message).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message);
		}

		[Fact]
		public async void WebsocketRequestCanBeCancelled() {
			var graphQLRequest = new GraphQLRequest(@"
				query Long {
					longRunning
				}");

			var chatQuery = Fixture.Server.Services.GetService<ChatQuery>();
			var cts = new CancellationTokenSource();

			await ChatClient.InitializeWebsocketConnection();
			var request =
				ConcurrentTaskWrapper.New(() => ChatClient.SendQueryAsync(graphQLRequest, () => new { longRunning = string.Empty }, cts.Token));

			// Test regular request
			// start request
			request.Start();
			// wait until the query has reached the server
			chatQuery.WaitingOnQueryBlocker.Wait(500).Should().BeTrue("because the request should have reached the server by then");
			// unblock the query
			chatQuery.LongRunningQueryBlocker.Set();
			// check execution time
			request.Invoking().ExecutionTime().Should().BeLessThan(100.Milliseconds());
			request.Invoke().Result.Data.longRunning.Should().Be("finally returned");

			// reset stuff
			chatQuery.LongRunningQueryBlocker.Reset();
			request.Clear();

			// cancellation test
			request.Start();
			chatQuery.WaitingOnQueryBlocker.Wait(500).Should().BeTrue("because the request should have reached the server by then");
			cts.Cancel();
			FluentActions.Awaiting(() => request.Invoking().Should().ThrowAsync<TaskCanceledException>("because the request was cancelled"))
				.ExecutionTime().Should().BeLessThan(100.Milliseconds());

			// let the server finish its query
			chatQuery.LongRunningQueryBlocker.Set();
		}
		
		[Fact]
		public async void CanHandleRequestErrorViaWebsocket() {
			await ChatClient.InitializeWebsocketConnection();
			var response = await ChatClient.SendQueryAsync<object>("this query is formatted quite badly").ConfigureAwait(false);
			response.Errors.Should().ContainSingle("because the query is invalid");
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
			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
			await ChatClient.InitializeWebsocketConnection();
			callbackMonitor.Should().HaveBeenInvokedWithPayload();

			Debug.WriteLine("creating subscription stream");
			var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

			Debug.WriteLine("subscribing...");
			using var tester = observable.Monitor();
			const string message1 = "Hello World";

			var response = await ChatClient.AddMessageAsync(message1).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message1);
			tester.Should().HaveReceivedPayload(TimeSpan.FromSeconds(3))
				.Which.Data.MessageAdded.Content.Should().Be(message1);

			const string message2 = "lorem ipsum dolor si amet";
			response = await ChatClient.AddMessageAsync(message2).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message2);
			tester.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message2);

			// disposing the client should throw a TaskCanceledException on the subscription
			ChatClient.Dispose();
			tester.Should().HaveCompleted();
		}

		public class MessageAddedSubscriptionResult {
			public MessageAddedContent MessageAdded { get; set; }

			public class MessageAddedContent {
				public string Content { get; set; }
			}
		}


		[Fact]
		public async void CanReconnectWithSameObservable() {
			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();

			Debug.WriteLine("creating subscription stream");
			var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest);

			Debug.WriteLine("subscribing...");
			var tester = observable.Monitor();
			callbackMonitor.Should().HaveBeenInvokedWithPayload();

			const string message1 = "Hello World";
			var response = await ChatClient.AddMessageAsync(message1).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message1);
			tester.Should().HaveReceivedPayload(3.Seconds())
				.Which.Data.MessageAdded.Content.Should().Be(message1);

			const string message2 = "How are you?";
			response = await ChatClient.AddMessageAsync(message2).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message2);
			tester.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message2);

			Debug.WriteLine("disposing subscription...");
			tester.Dispose(); // does not close the websocket connection

			Debug.WriteLine("creating new subscription...");
			var tester2 = observable.Monitor();
			tester2.Should().HaveReceivedPayload(TimeSpan.FromSeconds(10))
				.Which.Data.MessageAdded.Content.Should().Be(message2);

			const string message3 = "lorem ipsum dolor si amet";
			response = await ChatClient.AddMessageAsync(message3).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message3);
			tester2.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message3);

			// disposing the client should complete the subscription
			ChatClient.Dispose();
			tester2.Should().HaveCompleted();
			tester2.Dispose();
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

			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
			await ChatClient.InitializeWebsocketConnection();
			callbackMonitor.Should().HaveBeenInvokedWithPayload();

			Debug.WriteLine("creating subscription stream");
			var observable1 = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, callbackTester.Invoke);
			var observable2 = ChatClient.CreateSubscriptionStream<UserJoinedSubscriptionResult>(SubscriptionRequest2, callbackTester2.Invoke);

			Debug.WriteLine("subscribing...");
			var tester = observable1.Monitor();
			var tester2 = observable2.Monitor();

			const string message1 = "Hello World";
			var response = await ChatClient.AddMessageAsync(message1).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message1);
			tester.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message1);
			
			var joinResponse = await ChatClient.JoinDeveloperUser().ConfigureAwait(false);
			joinResponse.Data.Join.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");

			var payload = tester2.Should().HaveReceivedPayload().Subject;
			payload.Data.UserJoined.Id.Should().Be("1", "because that's the id we sent with our mutation request");
			payload.Data.UserJoined.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");

			Debug.WriteLine("disposing subscription...");
			tester2.Dispose();

			const string message3 = "lorem ipsum dolor si amet";
			response = await ChatClient.AddMessageAsync(message3).ConfigureAwait(false);
			response.Data.AddMessage.Content.Should().Be(message3);
			tester.Should().HaveReceivedPayload()
				.Which.Data.MessageAdded.Content.Should().Be(message3);

			// disposing the client should complete the subscription
			ChatClient.Dispose();
			tester.Should().HaveCompleted();
		}


		[Fact]
		public async void CanHandleConnectionTimeout() {
			var errorMonitor = new CallbackMonitor<Exception>();
			var reconnectBlocker = new ManualResetEventSlim(false);

			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
			// configure back-off strategy to allow it to be controlled from within the unit test
			ChatClient.Options.BackOffStrategy = i => {
				reconnectBlocker.Wait();
				return TimeSpan.Zero;
			};

			var websocketStates = new ConcurrentQueue<GraphQLWebsocketConnectionState>();

			using (ChatClient.WebsocketConnectionState.Subscribe(websocketStates.Enqueue)) {
				websocketStates.Should().ContainSingle(state => state == GraphQLWebsocketConnectionState.Disconnected);

				Debug.WriteLine("creating subscription stream");
				var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(SubscriptionRequest, errorMonitor.Invoke);

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
				var response = await ChatClient.AddMessageAsync(message1).ConfigureAwait(false);
				response.Data.AddMessage.Content.Should().Be(message1);
				tester.Should().HaveReceivedPayload()
					.Which.Data.MessageAdded.Content.Should().Be(message1);

				Debug.WriteLine("stopping web host...");
				await Fixture.ShutdownServer();
				Debug.WriteLine("web host stopped...");

				errorMonitor.Should().HaveBeenInvokedWithPayload(TimeSpan.FromSeconds(10))
					.Which.Should().BeOfType<WebSocketException>();
				websocketStates.Should().Contain(GraphQLWebsocketConnectionState.Disconnected);

				Fixture.CreateServer();
				reconnectBlocker.Set();
				callbackMonitor.Should().HaveBeenInvokedWithPayload();
				websocketStates.Should().ContainInOrder(
					GraphQLWebsocketConnectionState.Disconnected,
					GraphQLWebsocketConnectionState.Connecting,
					GraphQLWebsocketConnectionState.Connected);

				// disposing the client should complete the subscription
				ChatClient.Dispose();
				tester.Should().HaveCompleted(TimeSpan.FromSeconds(5));
			}
		}


		[Fact]
		public async void CanHandleSubscriptionError() {
			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
			await ChatClient.InitializeWebsocketConnection();
			callbackMonitor.Should().HaveBeenInvokedWithPayload();
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse<object>> observable = ChatClient.CreateSubscriptionStream<object>(
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
				ChatClient.Dispose();
			}
			
		}


		[Fact]
		public async void CanHandleQueryErrorInSubscription() {
			var test = new GraphQLRequest("tset", new { test = "blaa" });

			var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
			await ChatClient.InitializeWebsocketConnection();
			callbackMonitor.Should().HaveBeenInvokedWithPayload();
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse<object>> observable = ChatClient.CreateSubscriptionStream<object>(
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
				ChatClient.Dispose();
			}
		}

	}
}
