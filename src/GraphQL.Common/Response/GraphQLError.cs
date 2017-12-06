namespace GraphQL.Common.Response {

	public class GraphQLError {

		public string Message { get; set; }

		public GraphQLLocation[] Locations { get; set; }

	}

}
