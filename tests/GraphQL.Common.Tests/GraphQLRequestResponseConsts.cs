using GraphQL.Common.Tests.Request;
using GraphQL.Common.Tests.Response;

namespace GraphQL.Common.Tests {

	public class GraphQLRequestResponseConsts {

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLRequestResponse FieldsRequestResponse1 { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.FieldsRequest1,
			Response = GraphQLResponseConsts.FieldsResponse1
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fields"/>
		/// </summary>
		public static GraphQLRequestResponse FieldsRequestResponse2 { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.FieldsRequest2,
			Response = GraphQLResponseConsts.FieldsResponse2
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLRequestResponse ArgumentsRequestResponse1 { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.ArgumentsRequest1,
			Response = GraphQLResponseConsts.ArgumentsResponse1
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#arguments"/>
		/// </summary>
		public static GraphQLRequestResponse ArgumentsRequestResponse2 { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.ArgumentsRequest2,
			Response = GraphQLResponseConsts.ArgumentsResponse2
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#aliases"/>
		/// </summary>
		public static GraphQLRequestResponse AliasesRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.AliasesRequest,
			Response = GraphQLResponseConsts.AliasesResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#fragments"/>
		/// </summary>
		public static GraphQLRequestResponse FragmentsRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.FragmentsRequest,
			Response = GraphQLResponseConsts.FragmentsResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#operation-name"/>
		/// </summary>
		public static GraphQLRequestResponse OperationNameRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.OperationNameRequest,
			Response = GraphQLResponseConsts.OperationNameResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#variables"/>
		/// </summary>
		public static GraphQLRequestResponse VariablesRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.VariablesRequest,
			Response = GraphQLResponseConsts.VariablesResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#directives"/>
		/// </summary>
		public static GraphQLRequestResponse DirectivesRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.DirectivesRequest,
			Response = GraphQLResponseConsts.DirectivesResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#mutations"/>
		/// </summary>
		public static GraphQLRequestResponse MutationsRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.MutationsRequest,
			Response = GraphQLResponseConsts.MutationsResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#inline-fragments"/>
		/// </summary>
		public static GraphQLRequestResponse InlineFragmentsRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.InlineFragmentsRequest,
			Response = GraphQLResponseConsts.InlineFragmentsResponse
		};

		/// <summary>
		/// <see href="http://graphql.org/learn/queries/#meta-fields"/>
		/// </summary>
		public static GraphQLRequestResponse MetaFieldsRequestResponse { get; } = new GraphQLRequestResponse {
			Request = GraphQLRequestConsts.MetaFieldsRequest,
			Response = GraphQLResponseConsts.MetaFieldsResponse
		};

	}

}
