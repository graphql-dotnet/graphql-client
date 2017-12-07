using GraphQL.Common.Request;

namespace GraphQL.Common.Tests.Request {

	public static class GraphQLRequestConsts {

		public static GraphQLRequest SchemaTypeNameQuery { get; } = new GraphQLRequest {
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

		public static GraphQLRequest PokemonPikachuQuery { get; } = new GraphQLRequest {
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
