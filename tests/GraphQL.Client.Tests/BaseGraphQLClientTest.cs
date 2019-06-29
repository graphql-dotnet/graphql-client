using System;
using System.Net.Http;
using GraphQL.Client.Http;
using GraphQL.Server.Test;
using Microsoft.AspNetCore.TestHost;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GraphQL.Client.Tests {

	public abstract class BaseGraphQLClientTest : IDisposable {

		protected IGraphQLClient GraphQLClient { get; }
		protected GraphQLHttpClient GraphQLHttpClient { get; }

		protected IGraphQLClient GraphQLClientSwapi { get; } = new GraphQLHttpClient("https://swapi.apis.guru/");
		protected GraphQLHttpClient GitHuntClient { get; } = new GraphQLHttpClient("http://api.githunt.com/graphql");

		private readonly HttpClient httpClient;
		private readonly TestServer testServer = new TestServer(Program.CreateHostBuilder());

		public BaseGraphQLClientTest() {
			this.httpClient = this.testServer.CreateClient();
			this.GraphQLHttpClient = this.httpClient.AsGraphQLClient(new GraphQLHttpClientOptions());
			this.GraphQLHttpClient.EndPoint=new Uri($"{this.testServer.BaseAddress}");
			this.GraphQLClient = this.GraphQLHttpClient;
		}

		public void Dispose() {
			this.GraphQLClient.Dispose();
			this.GraphQLHttpClient.Dispose();
			this.httpClient.Dispose();
			this.testServer.Dispose();
		}

	}

}
