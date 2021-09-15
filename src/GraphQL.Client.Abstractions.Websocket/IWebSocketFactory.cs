using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Abstractions.Websocket
{
    /// <summary>
    /// creates and returns a configured and connected <see cref="WebSocket"/> instance
    /// </summary>
    public interface IWebSocketFactory
    {
        Task<WebSocket> ConnectAsync(Uri webSocketUri, CancellationToken cancellationToken);
    }
}
