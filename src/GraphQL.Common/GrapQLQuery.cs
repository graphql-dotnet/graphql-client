using Newtonsoft.Json.Linq;

namespace GraphQL.Common {

	public class GraphQLQuery {

		public string Query { get; set; }

		public string OperationName { get; set; }

		public JObject Variables { get; set; }

	}
}
