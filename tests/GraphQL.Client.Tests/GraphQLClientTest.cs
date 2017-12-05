using System;
using GraphQL.Common;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientTest {

		public GraphQLClient GraphQLClient { get; set; }=new GraphQLClient(new Uri("https://graphql-pokemon.now.sh/"));

		public GraphQLClientTest() {

		}

		[Fact]
		public async void PostSchemaTypesNameFact() {
			var graphQLResponse = await this.GraphQLClient.PostAsync(ConstQueries.PokemonPikatchuQuery).ConfigureAwait(false);
			Assert.NotNull(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

	}

}
