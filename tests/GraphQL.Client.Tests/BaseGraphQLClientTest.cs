using System;
using GraphQL.Client.Http;

namespace GraphQL.Client.Tests {

	public abstract class BaseGraphQLClientTest : IDisposable {

		protected GraphQLClient GraphQLClient { get; } = new GraphQLClient("https://swapi.apis.guru/");
		protected IGraphQLClient GraphQLClientSwapi { get; } = new GraphQLHttpClient("https://swapi.apis.guru/");
		protected GraphQLHttpClient GitHuntClient { get; } = new GraphQLHttpClient("http://api.githunt.com/graphql");

		public void Dispose() {
			this.GraphQLClient.Dispose();
			this.GraphQLClientSwapi.Dispose();
			this.GitHuntClient.Dispose();
		}

	}

}
