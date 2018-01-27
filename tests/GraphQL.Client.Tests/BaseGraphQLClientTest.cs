using System;

namespace GraphQL.Client.Tests {

	public abstract class BaseGraphQLClientTest : IDisposable {

		protected GraphQLClient GraphQLClient { get; } = new GraphQLClient("https://swapi.apis.guru/");

		public void Dispose() =>
			this.GraphQLClient.Dispose();

	}

}
