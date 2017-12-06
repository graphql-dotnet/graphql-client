using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	public static class GraphQLClientExtensions {

		private static readonly GraphQLQuery IntrospectionQuery = new GraphQLQuery {
			Query = @"
				query IntrospectionQuery {
					__schema {
						queryType {
							name
						},
						mutationType {
							name
						},
						subscriptionType {
							name
						},
						types {
							...FullType
						},
						directives {
							name,
							description,
							args {
								...InputValue
							},
							onOperation,
							onFragment,
							onField
						}
					}
				}

				fragment FullType on __Type {
					kind,
					name,
					description,
					fields(includeDeprecated: true) {
						name,
						description,
						args {
							...InputValue
						},
						type {
							...TypeRef
						},
						isDeprecated,
						deprecationReason
					},
					inputFields {
						...InputValue
					},
					interfaces {
						...TypeRef
					},
					enumValues(includeDeprecated: true) {
						name,
						description,
						isDeprecated,
						deprecationReason
					},
					possibleTypes {
						...TypeRef
					}
				}

				fragment InputValue on __InputValue {
					name,
					description,
					type {
						...TypeRef
					},
					defaultValue
				}

				fragment TypeRef on __Type {
					kind,
					name,
					ofType {
						kind,
						name,
						ofType {
							kind,
							name,
							ofType {
								kind,
								name
							}
						}
					}
				}".Replace("\t","").Replace("\n", "").Replace("\r", ""),
			OperationName = "IntrospectionQuery",
			Variables = null
		};

		public static async Task<GraphQLResponse> GetIntrospectionQueryAsync(this GraphQLClient graphQLClient) =>
			await graphQLClient.GetAsync(IntrospectionQuery).ConfigureAwait(false);

		public static async Task<GraphQLResponse> PostIntrospectionQueryAsync(this GraphQLClient graphQLClient) =>
			await graphQLClient.PostAsync(IntrospectionQuery).ConfigureAwait(false);

	}

}
