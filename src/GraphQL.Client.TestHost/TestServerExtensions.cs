using System;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.TestHost
{
    public static class TestServerExtensions
    {
        public static GraphQLHttpClient CreateGraphQLHttpClient(this TestServer testServer, GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer)
            => new GraphQLHttpClient(options, serializer, testServer.CreateClient(), new TestServerWebSocketFactory(testServer));
    }
}
