using GraphQL.Common.Request;

namespace GraphQL.Client.Tests {

	public static class ConstQueries {

		public static readonly GraphQLRequest SchemaTypeNameQuery = new GraphQLRequest {
			Query =
				@"query Schema {
					__schema {
						types {
							name
						}
					}
				}",
			Variables = null
		};

		public static readonly GraphQLRequest PokemonPikachuQuery = new GraphQLRequest {
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
				}",
			Variables = null
		};

	}

}
