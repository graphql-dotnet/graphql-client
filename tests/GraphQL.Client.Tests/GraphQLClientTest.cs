using System;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientTest {

		public GraphQLClient GraphQLClient { get; set; } = new GraphQLClient(new Uri("https://graphql-pokemon.now.sh/"));

		[Fact]
		public async void PostIntrospectionQueryFact() {
			var graphQLResponse = await this.GraphQLClient.PostIntrospectionQueryAsync().ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

		[Fact]
		public async void PostSchemaTypesNameFact() {
			var graphQLResponse = await this.GraphQLClient.PostAsync(ConstQueries.SchemaTypeNameQuery).ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

		[Fact]
		public async void PostPokemonPicachuFact() {
			var graphQLResponse = await this.GraphQLClient.PostAsync(ConstQueries.PokemonPikachuQuery).ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

	}

}
