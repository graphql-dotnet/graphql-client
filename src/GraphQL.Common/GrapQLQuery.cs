using Newtonsoft.Json.Linq;

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
		/// The Name of the operation.
		/// Only required if multiple operations are present in the query.
		/// </summary>
		public string OperationName { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public JObject Variables { get; set; }

	}
}
