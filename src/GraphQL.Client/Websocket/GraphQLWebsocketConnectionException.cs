using System.Runtime.Serialization;

namespace GraphQL.Client.Http.Websocket;

[Serializable]
public class GraphQLWebsocketConnectionException : Exception
{
    public GraphQLWebsocketConnectionException()
    {
    }

    protected GraphQLWebsocketConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public GraphQLWebsocketConnectionException(string message) : base(message)
    {
    }

    public GraphQLWebsocketConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
