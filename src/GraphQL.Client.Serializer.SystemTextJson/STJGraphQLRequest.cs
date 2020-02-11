using System.Text.Json.Serialization;
using Dahomey.Json.Attributes;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class STJGraphQLRequest: GraphQLRequest {
		[JsonPropertyName(QueryKey)]
		public override string Query { get; set; }
		[JsonPropertyName(OperationNameKey)]
		public override string? OperationName { get; set; }
		[JsonPropertyName(VariablesKey)]
		public override object? Variables { get; set; }

		public STJGraphQLRequest() { }

		public STJGraphQLRequest(GraphQLRequest other) {
			Query = other.Query;
			OperationName = other.OperationName;
			Variables = other.Variables;
		}

	}
}
