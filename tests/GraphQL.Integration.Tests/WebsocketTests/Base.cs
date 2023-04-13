using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using FluentAssertions.Reactive;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public abstract class Base : IAsyncLifetime
{
    protected readonly ITestOutputHelper Output;
    protected readonly IntegrationServerTestFixture Fixture;
    protected GraphQLHttpClient? ChatClient;

    protected Base(ITestOutputHelper output, IntegrationServerTestFixture fixture)
    {
        Output = output;
        Fixture = fixture;
    }

    protected static ReceivedMessage InitialMessage = new()
    {
        Content = "initial message",
        SentAt = DateTime.Now,
        FromId = "1"
    };

    public async Task InitializeAsync()
    {
        await Fixture.CreateServer();
        // make sure the buffer always contains the same message
        Fixture.Server.Services.GetService<Chat>().AddMessage(InitialMessage);

        // then create the chat client
        ChatClient ??= Fixture.GetChatClient(options => options.UseWebSocketForQueriesAndMutations = true);
    }

    public Task DisposeAsync()
    {
        ChatClient?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async void CanSendRequestViaWebsocket()
    {
        await ChatClient.InitializeWebsocketConnection();
        const string message = "some random testing message";
        var response = await ChatClient.AddMessageAsync(message);
        response.Errors.Should().BeNullOrEmpty();
        response.Data.AddMessage.Content.Should().Be(message);
    }

    [Fact]
    public async void CanUseWebSocketScheme()
    {
        ChatClient.Options.EndPoint = ChatClient.Options.EndPoint.GetWebSocketUri();
        await ChatClient.InitializeWebsocketConnection();
        const string message = "some random testing message";
        var response = await ChatClient.AddMessageAsync(message);
        response.Errors.Should().BeNullOrEmpty();
        response.Data.AddMessage.Content.Should().Be(message);
    }

    [Fact]
    public async void CanUseDedicatedWebSocketEndpoint()
    {
        ChatClient.Options.WebSocketEndPoint = ChatClient.Options.EndPoint.GetWebSocketUri();
        ChatClient.Options.EndPoint = new Uri("http://bad-endpoint.test");
        ChatClient.Options.UseWebSocketForQueriesAndMutations = true;
        await ChatClient.InitializeWebsocketConnection();
        const string message = "some random testing message";
        var response = await ChatClient.AddMessageAsync(message);
        response.Errors.Should().BeNullOrEmpty();
        response.Data.AddMessage.Content.Should().Be(message);
    }

    [Fact]
    public async void CanUseDedicatedWebSocketEndpointWithoutHttpEndpoint()
    {
        ChatClient.Options.WebSocketEndPoint = ChatClient.Options.EndPoint.GetWebSocketUri();
        ChatClient.Options.EndPoint = null;
        ChatClient.Options.UseWebSocketForQueriesAndMutations = false;
        await ChatClient.InitializeWebsocketConnection();
        const string message = "some random testing message";
        var response = await ChatClient.AddMessageAsync(message);
        response.Data.AddMessage.Content.Should().Be(message);
    }

    [Fact]
    public async void WebsocketRequestCanBeCancelled()
    {
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
        await request.Invoking().Should().ThrowAsync<TaskCanceledException>("because the request was cancelled");

        // let the server finish its query
        chatQuery.LongRunningQueryBlocker.Set();
    }

    [Fact]
    public async void CanHandleRequestErrorViaWebsocket()
    {
        await ChatClient.InitializeWebsocketConnection();
        var response = await ChatClient.SendQueryAsync<object>("this query is formatted quite badly");
        response.Errors.Should().ContainSingle("because the query is invalid");
    }

    private const string SUBSCRIPTION_QUERY = @"
			subscription {
			  messageAdded{
			    content
			  }
			}";

    private readonly GraphQLRequest _subscriptionRequest = new(SUBSCRIPTION_QUERY);

    [Fact]
    public async void CanCreateObservableSubscription()
    {
        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
        await ChatClient.InitializeWebsocketConnection();
        callbackMonitor.Should().HaveBeenInvokedWithPayload();

        Debug.WriteLine("creating subscription stream");
        var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(_subscriptionRequest);

        Debug.WriteLine("subscribing...");
        using var observer = observable.Observe();
        await observer.Should().PushAsync(1);
        observer.RecordedMessages.Last().Errors.Should().BeNullOrEmpty();
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(InitialMessage.Content);

        const string message1 = "Hello World";
        var response = await ChatClient.AddMessageAsync(message1);
        response.Errors.Should().BeNullOrEmpty();
        response.Data.AddMessage.Content.Should().Be(message1);
        await observer.Should().PushAsync(2);
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message1);

        const string message2 = "lorem ipsum dolor si amet";
        response = await ChatClient.AddMessageAsync(message2);
        response.Data.AddMessage.Content.Should().Be(message2);
        await observer.Should().PushAsync(3);
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message2);

        // disposing the client should throw a TaskCanceledException on the subscription
        ChatClient.Dispose();
        await observer.Should().CompleteAsync();
    }

    public class MessageAddedSubscriptionResult
    {
        public MessageAddedContent MessageAdded { get; set; }

        public class MessageAddedContent
        {
            public string Content { get; set; }
        }
    }

    [Fact]
    public async void CanReconnectWithSameObservable()
    {
        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();

        Debug.WriteLine("creating subscription stream");
        var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(_subscriptionRequest);

        Debug.WriteLine("subscribing...");
        var observer = observable.Observe();
        callbackMonitor.Should().HaveBeenInvokedWithPayload();
        await ChatClient.InitializeWebsocketConnection();
        Debug.WriteLine("websocket connection initialized");
        await observer.Should().PushAsync(1);
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(InitialMessage.Content);

        const string message1 = "Hello World";
        Debug.WriteLine($"adding message {message1}");
        var response = await ChatClient.AddMessageAsync(message1);
        response.Data.AddMessage.Content.Should().Be(message1);
        await observer.Should().PushAsync(2);
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message1);

        const string message2 = "How are you?";
        response = await ChatClient.AddMessageAsync(message2);
        response.Data.AddMessage.Content.Should().Be(message2);
        await observer.Should().PushAsync(3);
        observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message2);

        Debug.WriteLine("disposing subscription...");
        observer.Dispose(); // does not close the websocket connection

        Debug.WriteLine($"creating new subscription from thread {Environment.CurrentManagedThreadId} ...");
        var observer2 = observable.Observe();
        Debug.WriteLine($"waiting for payload on {Environment.CurrentManagedThreadId} ...");
        await observer2.Should().PushAsync(1);
        observer2.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message2);

        const string message3 = "lorem ipsum dolor si amet";
        response = await ChatClient.AddMessageAsync(message3);
        response.Data.AddMessage.Content.Should().Be(message3);
        await observer2.Should().PushAsync(2);
        observer2.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message3);

        // disposing the client should complete the subscription
        ChatClient.Dispose();
        await observer2.Should().CompleteAsync();
        observer2.Dispose();
    }

    private const string SUBSCRIPTION_QUERY2 = @"
			subscription {
			  userJoined{
				displayName
				id
			  }
			}";

    public class UserJoinedSubscriptionResult
    {
        public UserJoinedContent UserJoined { get; set; }

        public class UserJoinedContent
        {
            public string DisplayName { get; set; }

            public string Id { get; set; }
        }

    }

    private readonly GraphQLRequest _subscriptionRequest2 = new(SUBSCRIPTION_QUERY2);

    [Fact]
    public async void CanConnectTwoSubscriptionsSimultaneously()
    {
        int port = NetworkHelpers.GetFreeTcpPortNumber();
        var callbackTester = new CallbackMonitor<Exception>();
        var callbackTester2 = new CallbackMonitor<Exception>();

        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
        await ChatClient.InitializeWebsocketConnection();
        callbackMonitor.Should().HaveBeenInvokedWithPayload();

        Debug.WriteLine("creating subscription stream");
        var observable1 = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(_subscriptionRequest, callbackTester.Invoke);
        var observable2 = ChatClient.CreateSubscriptionStream<UserJoinedSubscriptionResult>(_subscriptionRequest2, callbackTester2.Invoke);

        Debug.WriteLine("subscribing...");
        var blocker = new ManualResetEventSlim(false);
        FluentTestObserver<GraphQLResponse<MessageAddedSubscriptionResult>> messagesMonitor = null;
        FluentTestObserver<GraphQLResponse<UserJoinedSubscriptionResult>> joinedMonitor = null;

        var tasks = new List<Task>
        {
            Task.Run(() =>
            {
                blocker.Wait();
                messagesMonitor = observable1.Observe();
            }),
            Task.Run(() =>
            {
                blocker.Wait();
                joinedMonitor = observable2.Observe();
            })
        };

        blocker.Set();
        await Task.WhenAll(tasks);

        await messagesMonitor.Should().PushAsync(1);
        messagesMonitor.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(InitialMessage.Content);

        const string message1 = "Hello World";
        var response = await ChatClient.AddMessageAsync(message1);
        response.Data.AddMessage.Content.Should().Be(message1);
        await messagesMonitor.Should().PushAsync(2);
        messagesMonitor.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message1);

        joinedMonitor.Should().NotPush();
        messagesMonitor.Clear();
        joinedMonitor.Clear();

        var joinResponse = await ChatClient.JoinDeveloperUser();
        joinResponse.Data.Join.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");

        var payload = await joinedMonitor.Should().PushAsync().GetLastMessageAsync();
        using (new AssertionScope())
        {
            payload.Data.UserJoined.Id.Should().Be("1", "because that's the id we sent with our mutation request");
            payload.Data.UserJoined.DisplayName.Should().Be("developer", "because that's the display name of user \"1\"");
        }

        messagesMonitor.Should().NotPush();
        messagesMonitor.Clear();
        joinedMonitor.Clear();

        Debug.WriteLine("disposing subscription...");
        joinedMonitor.Dispose();

        const string message3 = "lorem ipsum dolor si amet";
        response = await ChatClient.AddMessageAsync(message3);
        response.Data.AddMessage.Content.Should().Be(message3);
        var msg = await messagesMonitor.Should().PushAsync().GetLastMessageAsync();
        msg.Data.MessageAdded.Content.Should().Be(message3);

        // disposing the client should complete the subscription
        ChatClient.Dispose();
        await messagesMonitor.Should().CompleteAsync();
    }


    [Fact]
    public async void CanHandleConnectionTimeout()
    {
        var errorMonitor = new CallbackMonitor<Exception>();
        var reconnectBlocker = new ManualResetEventSlim(false);

        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
        // configure back-off strategy to allow it to be controlled from within the unit test
        ChatClient.Options.BackOffStrategy = i =>
        {
            Debug.WriteLine("back-off strategy: waiting on reconnect blocker");
            reconnectBlocker.Wait();
            Debug.WriteLine("back-off strategy: reconnecting...");
            return TimeSpan.Zero;
        };

        var websocketStates = new ConcurrentQueue<GraphQLWebsocketConnectionState>();

        using (ChatClient.WebsocketConnectionState.Subscribe(websocketStates.Enqueue))
        {
            websocketStates.Should().ContainSingle(state => state == GraphQLWebsocketConnectionState.Disconnected);

            Debug.WriteLine($"Test method thread id: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine("creating subscription stream");
            var observable = ChatClient.CreateSubscriptionStream<MessageAddedSubscriptionResult>(_subscriptionRequest, errorMonitor.Invoke);

            Debug.WriteLine("subscribing...");
            var observer = observable.Observe();
            callbackMonitor.Should().HaveBeenInvokedWithPayload();

            websocketStates.Should().ContainInOrder(
                GraphQLWebsocketConnectionState.Disconnected,
                GraphQLWebsocketConnectionState.Connecting,
                GraphQLWebsocketConnectionState.Connected);
            // clear the collection so the next tests on the collection work as expected
            websocketStates.Clear();

            await observer.Should().PushAsync(1);
            observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(InitialMessage.Content);

            const string message1 = "Hello World";
            var response = await ChatClient.AddMessageAsync(message1);
            response.Data.AddMessage.Content.Should().Be(message1);
            await observer.Should().PushAsync(2);
            observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(message1);

            Debug.WriteLine("stopping web host...");
            await Fixture.ShutdownServer();
            Debug.WriteLine("web host stopped");

            errorMonitor.Should().HaveBeenInvokedWithPayload(10.Seconds())
                .Which.Should().BeOfType<WebSocketException>();
            websocketStates.Should().Contain(GraphQLWebsocketConnectionState.Disconnected);

            Debug.WriteLine("restarting web host...");
            await InitializeAsync();
            Debug.WriteLine("web host started");
            reconnectBlocker.Set();
            callbackMonitor.Should().HaveBeenInvokedWithPayload(3.Seconds());
            await observer.Should().PushAsync(3);
            observer.RecordedMessages.Last().Data.MessageAdded.Content.Should().Be(InitialMessage.Content);

            websocketStates.Should().ContainInOrder(
                GraphQLWebsocketConnectionState.Disconnected,
                GraphQLWebsocketConnectionState.Connecting,
                GraphQLWebsocketConnectionState.Connected);

            // disposing the client should complete the subscription
            ChatClient.Dispose();
            await observer.Should().CompleteAsync(5.Seconds());
        }
    }

    [Fact]
    public async void CanHandleSubscriptionError()
    {
        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
        await ChatClient.InitializeWebsocketConnection();
        callbackMonitor.Should().HaveBeenInvokedWithPayload();
        Debug.WriteLine("creating subscription stream");
        var observable = ChatClient.CreateSubscriptionStream<object>(
            new GraphQLRequest(@"
					subscription {
					  failImmediately {
					    content
					  }
					}")
            );

        Debug.WriteLine("subscribing...");

        using var observer = observable.Observe();

        await observer.Should().PushAsync();
        observer.RecordedMessages.Last().Errors.Should().ContainSingle();

        await observer.Should().CompleteAsync();
        ChatClient.Dispose();
    }

    [Fact]
    public async void CanHandleQueryErrorInSubscription()
    {
        var callbackMonitor = ChatClient.ConfigureMonitorForOnWebsocketConnected();
        await ChatClient.InitializeWebsocketConnection();
        callbackMonitor.Should().HaveBeenInvokedWithPayload();
        Debug.WriteLine("creating subscription stream");
        var observable = ChatClient.CreateSubscriptionStream<object>(
            new GraphQLRequest(@"
					subscription {
					  fieldDoesNotExist {
					    content
					  }
					}")
        );

        Debug.WriteLine("subscribing...");

        using var observer = observable.Observe();

        await observer.Should().PushAsync();
        observer.RecordedMessages.Last().Errors.Should().ContainSingle();

        await observer.Should().CompleteAsync();
        ChatClient.Dispose();
    }
}
