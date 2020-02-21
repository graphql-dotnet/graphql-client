using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Newtonsoft {
	public class GraphQLRequest: GraphQL.GraphQLRequest {
		[JsonProperty(QueryKey)]
		public override string Query { get; set; }
		[JsonProperty(OperationNameKey)]
		public override string? OperationName { get; set; }
		[JsonProperty(VariablesKey)]
		public override object? Variables { get; set; }

		public GraphQLRequest() { }

		public GraphQLRequest(GraphQL.GraphQLRequest other) {
			Query = other.Query;
			OperationName = other.OperationName;
			Variables = other.Variables;
		}

	}
}
