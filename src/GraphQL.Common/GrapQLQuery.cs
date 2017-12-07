using Newtonsoft.Json.Linq;

namespace GraphQL.Common {

	/// <summary>
	/// Represents a Query that can be fetched to a GraphQL Server
	/// </summary>
	public class GraphQLQuery {

		/// <summary>
		/// The Query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// The Name of the operation
		/// </summary>
		public string? OperationName { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public JObject? Variables { get; set; }

	}
}
