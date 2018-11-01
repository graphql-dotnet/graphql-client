using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;


namespace GraphQL.Integration.Tests
{
	public class SubscriptionsTest: IClassFixture<WebApplicationFactory<IntegrationTestServer.Startup>>
	{

		private readonly WebApplicationFactory<IntegrationTestServer.Startup> _factory;

		public SubscriptionsTest(WebApplicationFactory<IntegrationTestServer.Startup> factory)
		{
			_factory = factory;
		}

		private GraphQLHttpClient GetGraphQLClient()
			=> _factory.CreateGraphQlHttpClient("graphql");


		[Fact]
		public async void AssertTestingHarness()
		{
			var client = GetGraphQLClient();
			var datetime = DateTime.Now;

			var response = await client.AddMessageAsync("Lorem ipsum dolor si amet").ConfigureAwait(false);

			Assert.Equal("Lorem ipsum dolor si amet", (string) response.Data.addMessage.content);
		}

		[Fact]
		public async void CanCreateObservableSubscription()
		{
			var client = GetGraphQLClient();

			var graphQLRequest = new GraphQLRequest
			{
				Query = @"
				subscription {
				  messageAdded{
				    content
				  }
				}"
			};

			Debug.WriteLine("creating subscription stream");
			IObservable<GraphQLResponse> observable = client.CreateSubscriptionStream(graphQLRequest);
			string message = null;

			await Task.Delay(1000).ConfigureAwait(false);

			Debug.WriteLine("subscribing...");
			using (observable.Subscribe(response => message = (string) response.Data.messageAdded.content, ex => throw ex))
			{
				await Task.Delay(10000).ConfigureAwait(false);
				Assert.Null(message);
				var response = await client.AddMessageAsync("Lorem ipsum dolor si amet").ConfigureAwait(false);
				Assert.Equal("Lorem ipsum dolor si amet", message);
			}
		}
	}
}
