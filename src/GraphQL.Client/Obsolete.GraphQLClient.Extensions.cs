using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// Extension Methods for <see cref="GraphQLClient"/>
	/// </summary>
	[Obsolete]
	public static class GraphQLClientExtensions {

		private const string IntrospectionQuery = @"
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
				}";

		private static readonly GraphQLRequest IntrospectionGraphQLRequest = new GraphQLRequest {
			Query = IntrospectionQuery.Replace("\t", "").Replace("\n", "").Replace("\r", ""),
			Variables = null
		};

		/// <summary>
		/// Send an IntrospectionQuery via GET
		/// </summary>
		/// <param name="graphQLClient">The GraphQLClient</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The GraphQLResponse</returns>
		public static Task<GraphQLResponse> GetIntrospectionQueryAsync(this GraphQLClient graphQLClient, CancellationToken cancellationToken = default) =>
			graphQLClient.GetAsync(IntrospectionGraphQLRequest, cancellationToken);

		/// <summary>
		/// Send an IntrospectionQuery via POST
		/// </summary>
		/// <param name="graphQLClient">The GraphQLClient</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The GraphQLResponse</returns>
		public static Task<GraphQLResponse> PostIntrospectionQueryAsync(this GraphQLClient graphQLClient, CancellationToken cancellationToken = default) =>
			graphQLClient.PostAsync(IntrospectionGraphQLRequest, cancellationToken);

	}

}
