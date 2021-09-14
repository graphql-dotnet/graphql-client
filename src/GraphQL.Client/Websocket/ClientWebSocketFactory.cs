using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket
{
#if NETSTANDARD
    /// <summary>
    /// Default web socket factory for netstandard2.0
    /// </summary>
    public class ClientWebSocketFactory : IWebSocketFactory
    {
        private readonly GraphQLHttpClientOptions _options;

        public ClientWebSocketFactory(GraphQLHttpClientOptions options)
        {
            _options = options;
        }

        public async Task<WebSocket> ConnectAsync(Uri webSocketUri, CancellationToken cancellationToken)
        {
            var webSocket = new ClientWebSocket();
            webSocket.Options.AddSubProtocol("graphql-ws");

            // the following properties are not supported in Blazor WebAssembly and throw a PlatformNotSupportedException error when accessed
            try
            {
                webSocket.Options.ClientCertificates = ((HttpClientHandler) _options.HttpMessageHandler).ClientCertificates;
            }
            catch (NotImplementedException)
            {
                Debug.WriteLine("property 'ClientWebSocketOptions.ClientCertificates' not implemented by current platform");
            }
            catch (PlatformNotSupportedException)
            {
                Debug.WriteLine("property 'ClientWebSocketOptions.ClientCertificates' not supported by current platform");
            }

            try
            {
                webSocket.Options.UseDefaultCredentials =
                    ((HttpClientHandler) _options.HttpMessageHandler).UseDefaultCredentials;
            }
            catch (NotImplementedException)
            {
                Debug.WriteLine("property 'ClientWebSocketOptions.UseDefaultCredentials' not implemented by current platform");
            }
            catch (PlatformNotSupportedException)
            {
                Debug.WriteLine("Property 'ClientWebSocketOptions.UseDefaultCredentials' not supported by current platform");
            }

            _options.ConfigureWebsocketOptions(webSocket.Options);

            Debug.WriteLine($"opening websocket {webSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})");
            await webSocket.ConnectAsync(webSocketUri, cancellationToken);
            return webSocket;
        }
    }
#endif
}
