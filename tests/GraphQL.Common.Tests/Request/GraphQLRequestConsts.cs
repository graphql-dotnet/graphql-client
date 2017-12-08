using GraphQL.Common.Request;

namespace GraphQL.Common.Tests.Request {

	public static class GraphQLRequestConsts {

		public static GraphQLRequest SchemaTypeNameRequest { get; } = new GraphQLRequest {
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

		public static GraphQLRequest PokemonPikachuRequest { get; } = new GraphQLRequest {
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

		public static GraphQLRequest HeroNameAndFriendsRequest { get; } = new GraphQLRequest {
			Query = @"
				query HeroNameAndFriends($episode: Episode) {
					hero(episode: $episode) {
						name,
						friends {
							name
						}
					}
				}",
			Variables = new {
				episode = "JEDI"
			}
		};

	}

}
