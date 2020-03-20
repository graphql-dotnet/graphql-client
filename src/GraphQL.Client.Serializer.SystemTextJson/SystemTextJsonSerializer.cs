using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public class SystemTextJsonSerializer : IGraphQLWebsocketJsonSerializer
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.SetupImmutableConverter();

        public JsonSerializerOptions Options { get; }

        public SystemTextJsonSerializer() : this(DefaultJsonSerializerOptions) { }

        public SystemTextJsonSerializer(Action<JsonSerializerOptions> configure) : this(configure.AndReturn(DefaultJsonSerializerOptions)) { }

        public SystemTextJsonSerializer(JsonSerializerOptions options)
        {
            Options = options;
            ConfigureMandatorySerializerOptions();
        }

        private void ConfigureMandatorySerializerOptions()
        {
            // deserialize extensions to Dictionary<string, object>
            Options.Converters.Insert(0, new GraphQLExtensionsConverter());
            // allow the JSON field "data" to match the property "Data" even without JsonNamingPolicy.CamelCase
            Options.PropertyNameCaseInsensitive = true;
        }

        public string SerializeToString(GraphQL.GraphQLRequest request)
        {
            return JsonSerializer.Serialize(request, Options);
        }

        public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<GraphQLResponse<TResponse>>(stream, Options, cancellationToken).AsTask();
        }

        public byte[] SerializeToBytes(Abstractions.Websocket.GraphQLWebSocketRequest request)
        {
            return JsonSerializer.SerializeToUtf8Bytes(request, Options);
        }

        public Task<WebsocketMessageWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream)
        {
            return JsonSerializer.DeserializeAsync<WebsocketMessageWrapper>(stream, Options).AsTask();
        }

        public GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<GraphQLWebSocketResponse<GraphQLResponse<TResponse>>>(new ReadOnlySpan<byte>(bytes),
                Options);
        }

    }
}
