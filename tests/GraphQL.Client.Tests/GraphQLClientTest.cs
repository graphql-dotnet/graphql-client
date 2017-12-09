using System;
using GraphQL.Common.Tests;
using GraphQL.Common.Tests.Request;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientTest {

		public GraphQLClient GraphQLClient { get; set; } = new GraphQLClient(new Uri("https://swapi.apis.guru/"));

		[Fact]
		public async void PostIntrospectionQueryFact() {
			var graphQLResponse = await this.GraphQLClient.PostIntrospectionQueryAsync().ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

	}

}
