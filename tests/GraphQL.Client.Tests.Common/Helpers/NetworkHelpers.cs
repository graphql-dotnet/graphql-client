using System.Net;
using System.Net.Sockets;

namespace GraphQL.Client.Tests.Common.Helpers;

public static class NetworkHelpers
{
    public static int GetFreeTcpPortNumber()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}
