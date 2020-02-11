using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Newtonsoft
{
    public class NewtonsoftJsonSerializer: IGraphQLWebsocketJsonSerializer
    {
	    public NewtonsoftJsonSerializerOptions Options { get; }

	    public NewtonsoftJsonSerializer()
	    {
			Options = new NewtonsoftJsonSerializerOptions();
		}
		public NewtonsoftJsonSerializer(Action<NewtonsoftJsonSerializerOptions> configure) {
			Options = new NewtonsoftJsonSerializerOptions();
			configure(Options);
		}

		public NewtonsoftJsonSerializer(NewtonsoftJsonSerializerOptions options) {
		    Options = options;
	    }


		public string SerializeToString(GraphQLRequest request) {
		    return JsonConvert.SerializeObject(request, Options.JsonSerializerSettings);
		}

		public byte[] SerializeToBytes(GraphQLWebSocketRequest request) {
			var json = JsonConvert.SerializeObject(request, Options.JsonSerializerSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public WebsocketResponseWrapper DeserializeToWebsocketResponseWrapper(Stream stream) {
			return DeserializeFromUtf8Stream<WebsocketResponseWrapper>(stream);
		}

		public GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes) {
			return JsonConvert.DeserializeObject<GraphQLWebSocketResponse<GraphQLResponse<TResponse>>>(Encoding.UTF8.GetString(bytes),
				Options.JsonSerializerSettings);
		}

		public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) {
			return Task.FromResult(DeserializeFromUtf8Stream<GraphQLResponse<TResponse>>(stream));
		}


		private T DeserializeFromUtf8Stream<T>(Stream stream) {
			using StreamReader sr = new StreamReader(stream);
			using JsonReader reader = new JsonTextReader(sr);
			JsonSerializer serializer = JsonSerializer.Create(Options.JsonSerializerSettings);
			return serializer.Deserialize<T>(reader);
		}

	}
}
