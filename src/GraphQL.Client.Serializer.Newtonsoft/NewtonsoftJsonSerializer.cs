using System.Text;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client.Serializer.Newtonsoft;

public class NewtonsoftJsonSerializer : IGraphQLWebsocketJsonSerializer
{
    public static JsonSerializerSettings DefaultJsonSerializerSettings => new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true },
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Converters = { new ConstantCaseEnumConverter() }
    };

    public JsonSerializerSettings JsonSerializerSettings { get; }

    public NewtonsoftJsonSerializer() : this(DefaultJsonSerializerSettings) { }

    public NewtonsoftJsonSerializer(Action<JsonSerializerSettings> configure) : this(configure.AndReturn(DefaultJsonSerializerSettings)) { }

    public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
    {
        JsonSerializerSettings = jsonSerializerSettings;
        ConfigureMandatorySerializerOptions();
    }

    // deserialize extensions to Dictionary<string, object>
    private void ConfigureMandatorySerializerOptions() => JsonSerializerSettings.Converters.Insert(0, new MapConverter());

    public string SerializeToString(GraphQLRequest request) => JsonConvert.SerializeObject(request, JsonSerializerSettings);

    public byte[] SerializeToBytes(GraphQLWebSocketRequest request)
    {
        string json = JsonConvert.SerializeObject(request, JsonSerializerSettings);
        return Encoding.UTF8.GetBytes(json);
    }

    public Task<WebsocketMessageWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream) => DeserializeFromUtf8Stream<WebsocketMessageWrapper>(stream);

    public GraphQLWebSocketResponse<TResponse> DeserializeToWebsocketResponse<TResponse>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        using JsonReader reader = new JsonTextReader(sr);
        var serializer = JsonSerializer.CreateDefault(JsonSerializerSettings);
        serializer.CheckAdditionalContent = true;
        return serializer.Deserialize<GraphQLWebSocketResponse<TResponse>>(reader);
    }

    public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) => DeserializeFromUtf8Stream<GraphQLResponse<TResponse>>(stream);

    private Task<T> DeserializeFromUtf8Stream<T>(Stream stream)
    {
        using var sr = new StreamReader(stream);
        using JsonReader reader = new JsonTextReader(sr);
        var serializer = JsonSerializer.Create(JsonSerializerSettings);
        return Task.FromResult(serializer.Deserialize<T>(reader));
    }
}
