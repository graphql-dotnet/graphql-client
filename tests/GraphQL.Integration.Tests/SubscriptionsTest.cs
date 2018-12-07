using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
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
				.ConfigureLogging((ctx, logging) => logging.AddDebug())
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

		private GraphQLHttpClient GetGraphQLClient(int port, Action<Exception> webSocketErrorHandler = null)
			=> new GraphQLHttpClient(new GraphQLHttpClientOptions
			{
				EndPoint = new Uri($"http://localhost:{port}/graphql"),
				WebSocketExceptionHandler = webSocketErrorHandler
			} );


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

		private const string SubscriptionQuery = @"
			subscription {
			  messageAdded{
			    content
			  }
			}";

		private readonly GraphQLRequest SubscriptionRequest = new GraphQLRequest
		{
			Query = SubscriptionQuery
		};

		[Fact]
		public async void CanCreateObservableSubscription()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);
				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse> observable = client.CreateSubscriptionStream(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();
				const string message1 = "Hello World";

				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string) response.Data.addMessage.content);

				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string) gqlResponse.Data.messageAdded.content.Value);
				});

				const string message2 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string) gqlResponse.Data.messageAdded.content.Value);
				});

				// disposing the client should throw a TaskCanceledException on the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
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
				IObservable<GraphQLResponse> observable = client.CreateSubscriptionStream(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = observable.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string) gqlResponse.Data.messageAdded.content.Value);
				});

				const string message2 = "How are you?";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string) gqlResponse.Data.messageAdded.content.Value);
				});

				Debug.WriteLine("disposing subscription...");
				tester.Dispose();

				Debug.WriteLine("creating new subscription...");
				tester = observable.SubscribeTester();
				tester.ShouldHaveReceivedUpdate(
					gqlResponse => { Assert.Equal(message2, (string) gqlResponse.Data.messageAdded.content.Value); },
					TimeSpan.FromSeconds(3));
				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, (string) response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message3, (string) gqlResponse.Data.messageAdded.content.Value);
				});

				// disposing the client should complete the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		private const string SubscriptionQuery2 = @"
			subscription {
			  messageAdded{
			    content
				from {
				  displayName
				}
			  }
			}";

		private readonly GraphQLRequest SubscriptionRequest2 = new GraphQLRequest
		{
			Query = SubscriptionQuery2
		};

		[Fact]
		public async void CanConnectMultipleSubscriptionsSimultaneously()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

				Debug.WriteLine("creating subscription stream");
				IObservable<GraphQLResponse> subscription1 = client.CreateSubscriptionStream(SubscriptionRequest);

				Debug.WriteLine("subscribing...");
				var tester = subscription1.SubscribeTester();

				const string message1 = "Hello World";
				var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
				Assert.Equal(message1, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string)gqlResponse.Data.messageAdded.content.Value);
				});

				IObservable<GraphQLResponse> subscription2 = client.CreateSubscriptionStream(SubscriptionRequest2);
				var tester2 = subscription2.SubscribeTester();
				tester2.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message1, (string)gqlResponse.Data.messageAdded.content.Value);
					Assert.Equal("tester", (string)gqlResponse.Data.messageAdded.from.displayName.Value);
				}, TimeSpan.FromSeconds(3));

				const string message2 = "How are you?";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string)gqlResponse.Data.messageAdded.content.Value);
				});
				tester2.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string)gqlResponse.Data.messageAdded.content.Value);
					Assert.Equal("tester", (string)gqlResponse.Data.messageAdded.from.displayName.Value);
				});

				Debug.WriteLine("disposing subscription...");
				tester.Dispose();

				const string message3 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message3).ConfigureAwait(false);
				Assert.Equal(message3, (string)response.Data.addMessage.content);
				tester2.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message3, (string)gqlResponse.Data.messageAdded.content.Value);
					Assert.Equal("tester", (string)gqlResponse.Data.messageAdded.from.displayName.Value);
				});

				// disposing the client should complete the subscription
				client.Dispose();
				tester2.ShouldHaveCompleted();
			}
		}

		[Fact]
		public async void CanHandleConnectionTimeout()
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			var server = CreateServer(port);
			var callbackTester = new CallbackTester<Exception>();

			var client = GetGraphQLClient(port, callbackTester.Callback);
			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse> observable = client.CreateSubscriptionStream(SubscriptionRequest);

			Debug.WriteLine("subscribing...");
			var tester = observable.SubscribeTester();
			const string message1 = "Hello World";

			var response = await client.AddMessageAsync(message1).ConfigureAwait(false);
			Assert.Equal(message1, (string)response.Data.addMessage.content);

			tester.ShouldHaveReceivedUpdate(gqlResponse =>
			{
				Assert.Equal(message1, (string)gqlResponse.Data.messageAdded.content.Value);
			});

			Debug.WriteLine("stopping web host...");
			await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
			Debug.WriteLine("web host stopped...");

			callbackTester.CallbackShouldHaveBeenInvoked(exception =>
			{
				Assert.IsType<WebSocketException>(exception);
			}, TimeSpan.FromSeconds(10));

			// disposing the client should complete the subscription
			client.Dispose();
			tester.ShouldHaveCompleted();
			server.Dispose();
		}
	}
}
