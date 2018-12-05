using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client.Http {

	/// <summary>
	/// The Options that the <see cref="GraphQLHttpClient"/> will use
	/// </summary>
	public class GraphQLHttpClientOptions {

		/// <summary>
		/// The GraphQL EndPoint to be used
		/// </summary>
		public Uri EndPoint { get; set; }

		/// <summary>
		/// The <see cref="Newtonsoft.Json.JsonSerializerSettings"/> that is going to be used
		/// </summary>
		public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Converters = new List<JsonConverter>
			{
				new StringEnumConverter()
			}
		};

		/// <summary>
		/// The <see cref="System.Net.Http.HttpMessageHandler"/> that is going to be used
		/// </summary>
		public HttpMessageHandler HttpMessageHandler { get; set; } = new HttpClientHandler();

		/// <summary>
		/// The <see cref="MediaTypeHeaderValue"/> that will be send on POST
		/// </summary>
		public MediaTypeHeaderValue MediaType { get; set; } = MediaTypeHeaderValue.Parse("application/json; charset=utf-8"); // This should be "application/graphql" also "application/x-www-form-urlencoded" is Accepted

		/// <summary>
		/// The exception handler for the websocket connection.
		/// <see cref="Exception"/>s thrown in this handler will bubble down through the reactive websocket stream and cause it to fail.
		/// All other exceptions will cause the websocket to be closed and to attempt a reconnection.
		/// </summary>
		public Action<Exception> WebSocketExceptionHandler { get; set; } =
			exception =>
			{
				if (exception is WebSocketException webSocketException)
				{
					Console.Error.WriteLine(webSocketException.ToString());
				}
				else
				{
					throw exception;
				}
			};
	}

}
