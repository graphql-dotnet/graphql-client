using System;
using System.Diagnostics;
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
		{}

		private GraphQLHttpClient GetGraphQLClient(int port)
			=> new GraphQLHttpClient($"http://localhost:{port}/graphql");


		[Fact]
		public async void AssertTestingHarness()
		{
			var port = 5002;
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

				const string message = "some random testing message";
				var response = await client.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, (string)response.Data.addMessage.content);
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
			var port = 5003;
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
				Assert.Equal(message2, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string)gqlResponse.Data.messageAdded.content.Value);
				});

				// disposing the client should throw a TaskCanceledException on the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}

		[Fact]
		public async void CanReconnectWithSameObservable()
		{
			var port = 5004;
			using (CreateServer(port))
			{
				var client = GetGraphQLClient(port);

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
				Debug.WriteLine("disposing subscription...");
				tester.Dispose();
				await Task.Delay(TimeSpan.FromSeconds(10));

				Debug.WriteLine("creating new subscription...");
				tester = observable.SubscribeTester();
				const string message2 = "lorem ipsum dolor si amet";
				response = await client.AddMessageAsync(message2).ConfigureAwait(false);
				Assert.Equal(message2, (string)response.Data.addMessage.content);
				tester.ShouldHaveReceivedUpdate(gqlResponse =>
				{
					Assert.Equal(message2, (string)gqlResponse.Data.messageAdded.content.Value);
				}, TimeSpan.FromSeconds(10));

				// disposing the client should complete the subscription
				client.Dispose();
				tester.ShouldHaveCompleted();
			}
		}
	}
}
