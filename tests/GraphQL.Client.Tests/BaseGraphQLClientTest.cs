using System;
using GraphQL.Client.Http;
using GraphQL.Server.Test;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Tests {

	public abstract class BaseGraphQLClientTest : IDisposable {

		protected IGraphQLClient ServerGraphQLClient { get; }

		protected GraphQLClient GraphQLClient { get; } = new GraphQLClient("https://swapi.apis.guru/");
		protected IGraphQLClient GraphQLClientSwapi { get; } = new GraphQLHttpClient("https://swapi.apis.guru/");
		protected GraphQLHttpClient GitHuntClient { get; } = new GraphQLHttpClient("http://api.githunt.com/graphql");

		private readonly TestServer testServer = new TestServer(Program.CreateHostBuilder());

		public BaseGraphQLClientTest() {
			var graphQlHttpClient = this.testServer.CreateClient().AsGraphQLClient(new GraphQLClientOptions{});
			graphQlHttpClient.EndPoint = new Uri($"{this.testServer.BaseAddress}");
			this.ServerGraphQLClient = graphQlHttpClient;
		}

		public void Dispose() {
			this.GraphQLClient.Dispose();
			this.GraphQLClientSwapi.Dispose();
			this.GitHuntClient.Dispose();

			this.ServerGraphQLClient.Dispose();
			this.testServer.Dispose();
		}

	}

}
