namespace GraphQL.Client.Abstractions.Websocket;

public interface IGraphQLWebSocketClient : IGraphQLClient
{
    /// <summary>
    /// The negotiated websocket sub-protocol. Will be <see langword="null"/> while no websocket connection is established.
    /// </summary>
    string? WebSocketSubProtocol { get; }

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

    /// <summary>
    /// Publishes the payload of all received pong messages (which may be <see langword="null"/>). Subscribing initiates the websocket connection. <br/>
    /// Ping/Pong is only supported when using the "graphql-transport-ws" websocket sub-protocol.
    /// </summary>
    /// <exception cref="NotSupportedException">the negotiated websocket sub-protocol does not support ping/pong</exception>
    IObservable<object?> PongStream { get; }

    /// <summary>
    /// Sends a ping to the server. <br/>
    /// Ping/Pong is only supported when using the "graphql-transport-ws" websocket sub-protocol.
    /// </summary>
    /// <exception cref="NotSupportedException">the negotiated websocket sub-protocol does not support ping/pong</exception>
    Task SendPingAsync(object? payload);

    /// <summary>
    /// Sends a pong to the server. This can be used for keep-alive scenarios (the client will automatically respond to pings received from the server). <br/>
    /// Ping/Pong is only supported when using the "graphql-transport-ws" websocket sub-protocol.
    /// </summary>
    /// <exception cref="NotSupportedException">the negotiated websocket sub-protocol does not support ping/pong</exception>
    Task SendPongAsync(object? payload);
}
