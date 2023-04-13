using System.Reflection;

namespace GraphQL.Client.Http.Websocket;
public static class WebSocketProtocols
{
    public const string AUTO_NEGOTIATE = null;

    //The WebSocket sub-protocol used for the [GraphQL over WebSocket Protocol](https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md).
    public const string GRAPHQL_TRANSPORT_WS = "graphql-transport-ws";

    //The deprecated subprotocol used by [subscriptions-transport-ws](https://github.com/apollographql/subscriptions-transport-ws).
    public const string GRAPHQL_WS = "graphql-ws";

    public static IEnumerable<string> GetSupportedWebSocketProtocols() =>
        typeof(WebSocketProtocols)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(info => (info.IsLiteral || info.IsInitOnly) && info.FieldType == typeof(string))
            .Select(f => f.IsLiteral ? (string)f.GetRawConstantValue() : (string)f.GetValue(null))
            .Where(s => s is not null);
}
