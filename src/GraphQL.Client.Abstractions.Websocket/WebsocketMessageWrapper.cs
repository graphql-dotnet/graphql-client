using System.Runtime.Serialization;

namespace GraphQL.Client.Abstractions.Websocket;

public class WebsocketMessageWrapper : GraphQLWebSocketResponse
{

    [IgnoreDataMember]
    public byte[] MessageBytes { get; set; }
}
