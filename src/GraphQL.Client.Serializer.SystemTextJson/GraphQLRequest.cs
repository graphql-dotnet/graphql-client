using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class GraphQLRequest: GraphQL.GraphQLRequest {
		[JsonPropertyName(QueryKey)]
		public override string Query { get; set; }
		[JsonPropertyName(OperationNameKey)]
		public override string? OperationName { get; set; }
		[JsonPropertyName(VariablesKey)]
		public override object? Variables { get; set; }

		public GraphQLRequest() { }

		public GraphQLRequest(GraphQL.GraphQLRequest other) {
			Query = other.Query;
			OperationName = other.OperationName;
			Variables = other.Variables;
		}

	}
}
