namespace GraphQL.Common.Request {

	public class GraphQLSubscriptionRequest {

		public string Id { get; set; }

		public string Type { get; set; }

		public GraphQLRequest Payload { get; set; }

	}

}
