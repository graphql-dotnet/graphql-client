using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represents the error of a <see cref="GraphQLResponse"/>
	/// </summary>
	public class GraphQLError {

		/// <summary>
		/// The error message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The Location of an error
		/// </summary>
		public GraphQLLocation[] Locations { get; set; }

		/// <summary>
		/// Additional error entries
		/// </summary>
		[JsonExtensionData]
		public IDictionary<string, JToken> AdditonalEntries { get; set; }

	}

}
