using System;
using System.IO;

namespace GraphQL.Client.Abstractions.Websocket
{
    public interface IGraphQLWebsocketJsonSerializer: IGraphQLJsonSerializer {
	    byte[] SerializeToBytes(GraphQLWebSocketRequest request);

	    WebsocketResponseWrapper DeserializeToWebsocketResponseWrapper(Stream stream);
	    GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes);

	}
}
