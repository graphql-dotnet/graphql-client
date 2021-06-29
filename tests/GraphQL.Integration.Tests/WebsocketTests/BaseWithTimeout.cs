using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.FluentAssertions.Reactive;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests
{
    public abstract class BaseWithTimeout:Base
    {

        protected BaseWithTimeout(ITestOutputHelper output, IntegrationServerTestFixture fixture) : base(output, fixture)
        {
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

                Debug.WriteLine($"Test method thread id: {Thread.CurrentThread.ManagedThreadId}");
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

                errorMonitor.Should().HaveBeenInvokedWithPayload(100.Seconds())
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
    }
}
