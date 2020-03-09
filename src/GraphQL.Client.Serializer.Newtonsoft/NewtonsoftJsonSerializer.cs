using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client.Serializer.Newtonsoft
{
    public class NewtonsoftJsonSerializer: IGraphQLWebsocketJsonSerializer
    {
		public static JsonSerializerSettings DefaultJsonSerializerSettings => new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true },
			MissingMemberHandling = MissingMemberHandling.Ignore
		};

		public JsonSerializerSettings JsonSerializerSettings { get; }

	    public NewtonsoftJsonSerializer() : this(DefaultJsonSerializerSettings) { }

		public NewtonsoftJsonSerializer(Action<JsonSerializerSettings> configure) : this(configure.AndReturn(DefaultJsonSerializerSettings)) { }

		public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings) {
		    JsonSerializerSettings = jsonSerializerSettings;
		    ConfigureMandatorySerializerOptions();
		}

		private void ConfigureMandatorySerializerOptions() {
			// deserialize extensions to Dictionary<string, object>
			JsonSerializerSettings.Converters.Insert(0, new GraphQLExtensionsConverter());
		}

		public string SerializeToString(GraphQL.GraphQLRequest request) {
		    return JsonConvert.SerializeObject(new GraphQLRequest(request), JsonSerializerSettings);
		}

		public byte[] SerializeToBytes(Abstractions.Websocket.GraphQLWebSocketRequest request) {
			var json = JsonConvert.SerializeObject(new GraphQLWebSocketRequest(request), JsonSerializerSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public Task<WebsocketMessageWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream) {
			return DeserializeFromUtf8Stream<WebsocketMessageWrapper>(stream);
		}

		public GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes) {
			return JsonConvert.DeserializeObject<GraphQLWebSocketResponse<GraphQLResponse<TResponse>>>(Encoding.UTF8.GetString(bytes),
				JsonSerializerSettings);
		}

		public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) {
			return DeserializeFromUtf8Stream<GraphQLResponse<TResponse>>(stream);
		}


		private Task<T> DeserializeFromUtf8Stream<T>(Stream stream) {
			using StreamReader sr = new StreamReader(stream);
			using JsonReader reader = new JsonTextReader(sr);
			JsonSerializer serializer = JsonSerializer.Create(JsonSerializerSettings);
			return Task.FromResult(serializer.Deserialize<T>(reader));
		}

	}
}
