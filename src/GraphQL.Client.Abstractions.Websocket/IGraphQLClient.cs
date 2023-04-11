namespace GraphQL.Client.Abstractions.Websocket;

public interface IGraphQLWebSocketClient : IGraphQLClient
{
    /// <summary>
    /// Publishes all exceptions which occur inside the websocket receive stream (i.e. for logging purposes)
    /// </summary>
    IObservable<Exception> WebSocketReceiveErrors { get; }

    /// <summary>
    /// Publishes the websocket connection state
    /// </summary>
    IObservable<GraphQLWebsocketConnectionState> WebsocketConnectionState { get; }

    /// <summary>
    /// Explicitly opens the websocket connection. Will be closed again on disposing the last subscription.
    /// </summary>
    Task InitializeWebsocketConnection();
}
