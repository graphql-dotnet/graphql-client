using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Newtonsoft
{
    public class NewtonsoftJsonSerializer: IGraphQLWebsocketJsonSerializer
    {
	    public NewtonsoftJsonSerializerOptions Options { get; }

	    public NewtonsoftJsonSerializer() : this(new NewtonsoftJsonSerializerOptions()) { }

		public NewtonsoftJsonSerializer(Action<NewtonsoftJsonSerializerOptions> configure) : this(configure.New()) { }

		public NewtonsoftJsonSerializer(NewtonsoftJsonSerializerOptions options) {
		    Options = options;
		    ConfigureMandatorySerializerOptions();
		}

		private void ConfigureMandatorySerializerOptions() {
			// deserialize extensions to Dictionary<string, object>
			Options.JsonSerializerSettings.Converters.Insert(0, new GraphQLExtensionsConverter());
		}

		public string SerializeToString(GraphQL.GraphQLRequest request) {
		    return JsonConvert.SerializeObject(new GraphQLRequest(request), Options.JsonSerializerSettings);
		}

		public byte[] SerializeToBytes(Abstractions.Websocket.GraphQLWebSocketRequest request) {
			var json = JsonConvert.SerializeObject(request, Options.JsonSerializerSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public Task<WebsocketResponseWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream) {
			return DeserializeFromUtf8Stream<WebsocketResponseWrapper>(stream);
		}

		public GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes) {
			return JsonConvert.DeserializeObject<GraphQLWebSocketResponse<GraphQLResponse<TResponse>>>(Encoding.UTF8.GetString(bytes),
				Options.JsonSerializerSettings);
		}

		public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) {
			return DeserializeFromUtf8Stream<GraphQLResponse<TResponse>>(stream);
		}


		private Task<T> DeserializeFromUtf8Stream<T>(Stream stream) {
			using StreamReader sr = new StreamReader(stream);
			using JsonReader reader = new JsonTextReader(sr);
			JsonSerializer serializer = JsonSerializer.Create(Options.JsonSerializerSettings);
			return Task.FromResult(serializer.Deserialize<T>(reader));
		}

	}
}
