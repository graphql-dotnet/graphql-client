using System;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.TestHost
{
    public static class TestServerExtensions
    {
        public static GraphQLHttpClient CreateGraphQLHttpClient(this TestServer server, GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer)
        {
            var testWebSocketClient = server.CreateWebSocketClient();
            testWebSocketClient.ConfigureRequest = r =>
            {
                r.Headers["Sec-WebSocket-Protocol"] = "graphql-ws";
            };

            return new GraphQLHttpClient(options, serializer, server.CreateClient(), (uri, token) => testWebSocketClient.ConnectAsync(uri, token));
        }
    }
}
