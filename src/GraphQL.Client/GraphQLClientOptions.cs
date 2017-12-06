using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client {

	public class GraphQLClientOptions {

		public Uri EndPoint { get; set; }

		public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public HttpClientHandler HttpClientHandler { get; set; } = new HttpClientHandler();

		public string MediaType { get; set; } = "application/json"; // This should be "application/graphql"

	}

}
