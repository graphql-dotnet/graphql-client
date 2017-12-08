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

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLRequest FieldsRequest1 { get; } = new GraphQLRequest {
			Query = @"
				{
					hero {
						name
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLRequest FieldsRequest2 { get; } = new GraphQLRequest {
			Query = @"
				{
					hero {
						name
						# Queries can have comments!
						friends {
							name
						}
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLRequest ArgumentsRequest1 { get; } = new GraphQLRequest {
			Query = @"
				{
					human(id: ""1000"") {
						name
						height
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLRequest ArgumentsRequest2 { get; } = new GraphQLRequest {
			Query = @"
				{
					human(id: ""1000"") {
						name
						height(unit: FOOT)
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#aliases"/>
		/// </summary>
		public static GraphQLRequest AliasesRequest { get; } = new GraphQLRequest {
			Query = @"
				{
					empireHero: hero(episode: EMPIRE) {
						name
					}
					jediHero: hero(episode: JEDI) {
						name
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fragments"/>
		/// </summary>
		public static GraphQLRequest FragmentsRequest { get; } = new GraphQLRequest {
			Query = @"
				{
					leftComparison: hero(episode: EMPIRE) {
						...comparisonFields
					}
					rightComparison: hero(episode: JEDI) {
						...comparisonFields
					}
				}

				fragment comparisonFields on Character {
					name
					appearsIn
					friends {
						name
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#operation-name"/>
		/// </summary>
		public static GraphQLRequest OperationNameRequest { get; } = new GraphQLRequest {
			Query = @"
				query HeroNameAndFriends {
					hero {
						name
						friends {
							name
						}
					}
				}",
			Variables = null
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#variables"/>
		/// </summary>
		public static GraphQLRequest VariablesRequest { get; } = new GraphQLRequest {
			Query = @"
				query HeroNameAndFriends($episode: Episode) {
					hero(episode: $episode) {
						name
						friends {
							name
						}
					}
				}",
			Variables = new {
				episode = "JEDI"
			}
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#directives"/>
		/// </summary>
		public static GraphQLRequest DirectivesRequest { get; } = new GraphQLRequest {
			Query = @"
				query Hero($episode: Episode, $withFriends: Boolean!) {
					hero(episode: $episode) {
						name
						friends @include(if: $withFriends) {
							name
						}
					}
				}",
			Variables = new {
				episode = "JEDI",
				withFriends = false
			}
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#mutations"/>
		/// </summary>
		public static GraphQLRequest MutationsRequest { get; } = new GraphQLRequest {
			Query = @"
				mutation CreateReviewForEpisode($ep: Episode!, $review: ReviewInput!) {
					createReview(episode: $ep, review: $review) {
						stars
						commentary
					}
				}",
			Variables = new {
				ep = "JEDI",
				review = new {
					stars=5,
					commentary="This is a great movie!"
				}
			}
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#inline-fragments"/>
		/// </summary>
		public static GraphQLRequest InlineFragmentsRequest { get; } = new GraphQLRequest {
			Query = @"
				query HeroForEpisode($ep: Episode!) {
					hero(episode: $ep) {
						name
						... on Droid {
							primaryFunction
						}
						... on Human {
							height
						}
					}
				}",
			Variables = new {
				ep = "JEDI"
			}
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#meta-fields"/>
		/// </summary>
		public static GraphQLRequest MetaFieldsRequest { get; } = new GraphQLRequest {
			Query = @"
				{
					search(text: ""an"") {
						__typename
						...on Human {
							name
						}
						... on Droid {
							name
						}
						... on Starship {
							name
						}
					}
				}",
			Variables = null
		};

	}

}
