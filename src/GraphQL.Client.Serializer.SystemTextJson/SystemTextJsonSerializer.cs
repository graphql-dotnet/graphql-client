using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Serializer.SystemTextJson;

public class SystemTextJsonSerializer : IGraphQLWebsocketJsonSerializer
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(new ConstantCaseJsonNamingPolicy(), false) }
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
        Options.Converters.Insert(0, new ErrorPathConverter());
        Options.Converters.Insert(0, new MapConverter());
        // allow the JSON field "data" to match the property "Data" even without JsonNamingPolicy.CamelCase
        Options.PropertyNameCaseInsensitive = true;
    }

    public string SerializeToString(GraphQLRequest request) => JsonSerializer.Serialize(request, Options);

    public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) => JsonSerializer.DeserializeAsync<GraphQLResponse<TResponse>>(stream, Options, cancellationToken).AsTask();

    public byte[] SerializeToBytes(GraphQLWebSocketRequest request) => JsonSerializer.SerializeToUtf8Bytes(request, Options);

    public Task<WebsocketMessageWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream) => JsonSerializer.DeserializeAsync<WebsocketMessageWrapper>(stream, Options).AsTask();

    public GraphQLWebSocketResponse<TResponse> DeserializeToWebsocketResponse<TResponse>(byte[] bytes) =>
        JsonSerializer.Deserialize<GraphQLWebSocketResponse<TResponse>>(new ReadOnlySpan<byte>(bytes),
            Options);
}
