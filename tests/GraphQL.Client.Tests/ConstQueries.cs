using GraphQL.Common;

namespace GraphQL.Client.Tests {

	public static class ConstQueries {

		public static readonly GraphQLQuery SchemaTypeNameQuery = new GraphQLQuery {
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

		public static readonly GraphQLQuery PokemonPikachuQuery = new GraphQLQuery {
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
