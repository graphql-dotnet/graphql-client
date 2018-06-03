namespace GraphQL.Common.Response {

	public class GraphQLSubscriptionResponse {

		public string Id { get; set; }

		public string Type { get; set; }

		public GraphQLResponse Payload { get; set; }

	}

}
