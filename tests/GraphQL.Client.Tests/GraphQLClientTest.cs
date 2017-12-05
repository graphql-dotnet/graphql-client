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
			var graphQLQuery = new GraphQLQuery {
				Query =
				@"{
					pokemon(name: ""Pikachu"") {
						id,
						number,
						name,
						attacks {
							special {
								name,
								type,
								damage
							}
						},
						evolutions {
							id,
							number,
							name,
							weight {
								minimum,
								maximum
							},
							attacks {
								fast {
									name,
									type,
									damage
								}
							}
						}
					}
				}"
			};
			var graphQLResponse = await this.GraphQLClient.PostAsync(graphQLQuery).ConfigureAwait(false);
			Assert.NotNull(graphQLResponse.Data);
			Assert.Null(graphQLResponse.Errors);
		}

	}

}
