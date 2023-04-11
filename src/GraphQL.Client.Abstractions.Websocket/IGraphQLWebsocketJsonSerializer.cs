namespace GraphQL.Client.Abstractions.Websocket;

/// <summary>
/// The json serializer interface for the graphql-dotnet http client.
/// Implementations should provide a parameterless constructor for convenient usage
/// </summary>
public interface IGraphQLWebsocketJsonSerializer : IGraphQLJsonSerializer
{
    byte[] SerializeToBytes(GraphQLWebSocketRequest request);

    Task<WebsocketMessageWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream);

    GraphQLWebSocketResponse<TResponse> DeserializeToWebsocketResponse<TResponse>(byte[] bytes);
}
