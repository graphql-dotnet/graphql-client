using System;
using GraphQL.Common.Tests;
using GraphQL.Common.Tests.Request;
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
			var graphQLResponse = await this.GraphQLClient.PostAsync(GraphQLRequestConsts.SchemaTypeNameRequest).ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

		[Fact]
		public async void PostPokemonPicachuFact() {
			var graphQLResponse = await this.GraphQLClient.PostAsync(GraphQLRequestConsts.PokemonPikachuRequest).ConfigureAwait(false);
			AssertGraphQL.CorrectGraphQLResponse(graphQLResponse);
		}

	}

}
