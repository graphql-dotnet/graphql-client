using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.TestHost
{
    public class TestServerWebSocketFactory: IWebSocketFactory
    {
        private readonly WebSocketClient _webSocketClient;

        public TestServerWebSocketFactory(TestServer testServer)
        {
            _webSocketClient = testServer.CreateWebSocketClient();
            _webSocketClient.ConfigureRequest = r =>
            {
                r.Headers["Sec-WebSocket-Protocol"] = "graphql-ws";
            };
        }

        public Task<WebSocket> ConnectAsync(Uri webSocketUri, CancellationToken cancellationToken)
            => _webSocketClient.ConnectAsync(webSocketUri, cancellationToken);
    }
}
