using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Common;

namespace GraphQL.Client.Tests {

	public static class ConstQueries {

		public static readonly GraphQLQuery PokemonPikatchuQuery = new GraphQLQuery {
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
			OperationName = null,
			Variables = null
		};
	}

}
