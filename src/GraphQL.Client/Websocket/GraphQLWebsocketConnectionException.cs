#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace GraphQL.Client.Http.Websocket;

[Serializable]
public class GraphQLWebsocketConnectionException : Exception
{
    public GraphQLWebsocketConnectionException()
    {
    }

    public GraphQLWebsocketConnectionException(string message) : base(message)
    {
    }

    public GraphQLWebsocketConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected GraphQLWebsocketConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif

}
