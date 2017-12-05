using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client {

	public class GraphQLClientOptions {

		public Uri EndPoint { get; set; }

		public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public string MediaType { get; set; } = "application/json"; // This should be "application/graphql"

	}

}
