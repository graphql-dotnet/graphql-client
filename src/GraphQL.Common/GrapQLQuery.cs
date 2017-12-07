namespace GraphQL.Common {

	/// <summary>
	/// Represents a Query that can be fetched to a GraphQL Server.
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#post-request"/>
	/// </summary>
	public class GraphQLQuery {

		/// <summary>
		/// The Query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public dynamic Variables { get; set; }

	}
}
