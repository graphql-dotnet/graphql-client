namespace GraphQL.Client.Response {

	public class GraphQLError {

		public string Message { get; set; }

		public GraphQLLocation[] Locations { get; set; }

	}

}
