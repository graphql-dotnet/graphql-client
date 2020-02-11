using System;
using System.IO;

namespace GraphQL.Client.Abstractions.Websocket
{
	/// <summary>
	/// The json serializer interface for the graphql-dotnet http client.
	/// Implementations should provide a parameterless constructor for convenient usage
	/// </summary>
    public interface IGraphQLWebsocketJsonSerializer: IGraphQLJsonSerializer {
	    byte[] SerializeToBytes(GraphQLWebSocketRequest request);

	    WebsocketResponseWrapper DeserializeToWebsocketResponseWrapper(Stream stream);
	    GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes);

	}
}
