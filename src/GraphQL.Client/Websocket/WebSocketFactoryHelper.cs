using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket
{
    public static class WebSocketFactoryHelper
    {
        public static IWebSocketFactory GetDefaultWebSocketFactory(GraphQLHttpClientOptions options)
        {
#if NETFRAMEWORK
            return new NetFrameworkWebSocketFactory(options);
#else
            return new ClientWebSocketFactory(options);
#endif
        }
    }
}
