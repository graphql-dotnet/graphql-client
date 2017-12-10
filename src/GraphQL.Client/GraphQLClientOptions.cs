using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client {

	public class GraphQLClientOptions {

		// TODO Cache

		/// <summary>
		/// The GraphQL EndPoint to be used
		/// </summary>
		public Uri EndPoint { get; set; }

		/// <summary>
		/// The <see cref="Newtonsoft.Json.JsonSerializerSettings"/> that is going to be used
		/// </summary>
		public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		/// <summary>
		/// The <see cref="System.Net.Http.HttpClientHandler"/> that is going to be used
		/// </summary>
		public HttpClientHandler HttpClientHandler { get; set; } = new HttpClientHandler();

		/// <summary>
		/// The <see cref="MediaTypeHeaderValue"/> that will be send on POST
		/// </summary>
		public string MediaType { get; set; } = "application/json"; // This should be "application/graphql" also "application/x-www-form-urlencoded" is Accepted

	}

}
