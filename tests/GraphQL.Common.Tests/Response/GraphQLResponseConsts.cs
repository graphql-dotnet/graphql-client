using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Common.Tests.Response {

	public class GraphQLResponseConsts {

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLResponse FieldsResponse1 { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""hero"": {
						""name"": ""R2-D2""
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLResponse FieldsResponse2 { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""hero"": {
						""name"": ""R2-D2"",
						""friends"": [
							{
								""name"": ""Luke Skywalker""
							},
							{
								""name"": ""Han Solo""
							},
							{
								""name"": ""Leia Organa""
							}
						]
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLResponse ArgumentsResponse1 { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""human"": {
						""name"": ""Luke Skywalker"",
						""height"": 1.72
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLResponse ArgumentsResponse2 { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""human"": {
						""name"": ""Luke Skywalker"",
						""height"": 5.6430448
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#aliases"/>
		/// </summary>
		public static GraphQLResponse AliasesResponse { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""empireHero"": {
						""name"": ""Luke Skywalker""
					},
					""jediHero"": {
						""name"": ""R2-D2""
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fragments"/>
		/// </summary>
		public static GraphQLResponse FragmentsResponse { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""leftComparison"": {
						""name"": ""Luke Skywalker"",
						""appearsIn"": [
							""NEWHOPE"",
							""EMPIRE"",
							""JEDI""
						],
						""friends"": [
							{
								""name"": ""Han Solo""
							},
							{
								""name"": ""Leia Organa""
							},
							{
								""name"": ""C-3PO""
							},
							{
								""name"": ""R2-D2""
							}
						]
					},
					""rightComparison"": {
						""name"": ""R2-D2"",
						""appearsIn"": [
							""NEWHOPE"",
							""EMPIRE"",
							""JEDI""
						],
						""friends"": [
							{
								""name"": ""Luke Skywalker""
							},
							{
								""name"": ""Han Solo""
							},
							{
								""name"": ""Leia Organa""
							}
						]
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#operation-name"/>
		/// </summary>
		public static GraphQLResponse OperationNameResponse { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""hero"": {
						""name"": ""R2-D2"",
						""friends"": [
							{
								""name"": ""Luke Skywalker""
							},
							{
								""name"": ""Han Solo""
							},
							{
								""name"": ""Leia Organa""
							}
						]
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#variables"/>
		/// </summary>
		public static GraphQLResponse VariablesResponse { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""hero"": {
						""name"": ""R2-D2"",
						""friends"": [
							{
								""name"": ""Luke Skywalker""
							},
							{
								""name"": ""Han Solo""
							},
							{
								""name"": ""Leia Organa""
							}
						]
					}
				}
			}");

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#directives"/>
		/// </summary>
		public static GraphQLResponse DirectivesResponse { get; } = JsonConvert.DeserializeObject<GraphQLResponse>(@"
			{
				""data"": {
					""hero"": {
						""name"": ""R2-D2""
					}
				}
			}");

	}

}
