using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket
{

#if NETFRAMEWORK
    /// <summary>
    /// Default web socket factory for net4.6.1 (including support for Windows 7 and Windows Server 2008)
    /// </summary>
    public class NetFrameworkWebSocketFactory : IWebSocketFactory
    {
        private readonly GraphQLHttpClientOptions _options;

        public NetFrameworkWebSocketFactory(GraphQLHttpClientOptions options)
        {
            _options = options;
        }

        public async Task<WebSocket> ConnectAsync(Uri webSocketUri, CancellationToken cancellationToken)
        {
            // fix websocket not supported on win 7 using
            // https://github.com/PingmanTools/System.Net.WebSockets.Client.Managed
            var webSocket = SystemClientWebSocket.CreateClientWebSocket();
            switch (webSocket) {
                case ClientWebSocket nativeWebSocket:
                    nativeWebSocket.Options.AddSubProtocol("graphql-ws");
                    nativeWebSocket.Options.ClientCertificates = ((HttpClientHandler)_options.HttpMessageHandler).ClientCertificates;
                    nativeWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)_options.HttpMessageHandler).UseDefaultCredentials;
                    _options.ConfigureWebsocketOptions(nativeWebSocket.Options);
                    break;
                case System.Net.WebSockets.Managed.ClientWebSocket managedWebSocket:
                    managedWebSocket.Options.AddSubProtocol("graphql-ws");
                    managedWebSocket.Options.ClientCertificates = ((HttpClientHandler)_options.HttpMessageHandler).ClientCertificates;
                    managedWebSocket.Options.UseDefaultCredentials = ((HttpClientHandler)_options.HttpMessageHandler).UseDefaultCredentials;
                    break;
                default:
                    throw new NotSupportedException($"unknown websocket type {webSocket.GetType().Name}");
            }

            Debug.WriteLine($"opening websocket {webSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})");
            await webSocket.ConnectAsync(webSocketUri, cancellationToken);
            return webSocket;
        }
    }
#endif
}
