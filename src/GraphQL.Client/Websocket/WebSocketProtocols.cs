namespace GraphQL.Client.Http.Websocket;
public static class WebSocketProtocols
{
    //The WebSocket sub-protocol used for the [GraphQL over WebSocket Protocol](https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md).
    public const string GRAPHQL_TRANSPORT_WS = "graphql-transport-ws";

    //The deprecated subprotocol used by [subscriptions-transport-ws](https://github.com/apollographql/subscriptions-transport-ws).
    public const string GRAPHQL_WS = "graphql-ws";
}
