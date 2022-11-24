namespace GraphQL.Client.Abstractions.Websocket;

public static class GraphQLWebSocketMessageType
{

    /// <summary>
    ///     Client sends this message after plain websocket connection to start the communication with the server
    ///     The server will response only with GQL_CONNECTION_ACK + GQL_CONNECTION_KEEP_ALIVE(if used) or GQL_CONNECTION_ERROR
    ///     to this message.
    ///     payload: Object : optional parameters that the client specifies in connectionParams
    /// </summary>
    public const string GQL_CONNECTION_INIT = "connection_init";

    /// <summary>
    ///     The server may responses with this message to the GQL_CONNECTION_INIT from client, indicates the server accepted
    ///     the connection.
    /// </summary>
    public const string GQL_CONNECTION_ACK = "connection_ack"; // Server -> Client

    /// <summary>
    ///     The server may responses with this message to the GQL_CONNECTION_INIT from client, indicates the server rejected
    ///     the connection.
    ///     It server also respond with this message in case of a parsing errors of the message (which does not disconnect the
    ///     client, just ignore the message).
    ///     payload: Object: the server side error
    /// </summary>
    public const string GQL_CONNECTION_ERROR = "connection_error"; // Server -> Client

    /// <summary>
    ///     Server message that should be sent right after each GQL_CONNECTION_ACK processed and then periodically to keep the
    ///     client connection alive.
    ///     The client starts to consider the keep alive message only upon the first received keep alive message from the
    ///     server.
    ///     <remarks>
    ///         NOTE: This one here don't follow the standard due to connection optimization
    ///     </remarks>
    /// </summary>
    public const string GQL_CONNECTION_KEEP_ALIVE = "ka"; // Server -> Client

    /// <summary>
    ///     Client sends this message to terminate the connection.
    /// </summary>
    public const string GQL_CONNECTION_TERMINATE = "connection_terminate"; // Client -> Server

    /// <summary>
    ///     Client sends this message to execute GraphQL operation
    ///     id: string : The id of the GraphQL operation to start
    ///     payload: Object:
    ///     query: string : GraphQL operation as string or parsed GraphQL document node
    ///     variables?: Object : Object with GraphQL variables
    ///     operationName?: string : GraphQL operation name
    /// </summary>
    public const string GQL_START = "start";

    /// <summary>
    ///     The server sends this message to transfer the GraphQL execution result from the server to the client, this message
    ///     is a response for GQL_START message.
    ///     For each GraphQL operation send with GQL_START, the server will respond with at least one GQL_DATA message.
    ///     id: string : ID of the operation that was successfully set up
    ///     payload: Object :
    ///     data: any: Execution result
    ///     errors?: Error[] : Array of resolvers errors
    /// </summary>
    public const string GQL_DATA = "data"; // Server -> Client

    /// <summary>
    ///     Server sends this message upon a failing operation, before the GraphQL execution, usually due to GraphQL validation
    ///     errors (resolver errors are part of GQL_DATA message, and will be added as errors array)
    ///     payload: Error : payload with the error attributed to the operation failing on the server
    ///     id: string : operation ID of the operation that failed on the server
    /// </summary>
    public const string GQL_ERROR = "error"; // Server -> Client

    /// <summary>
    ///     Server -> Client: Server sends this message to indicate that a GraphQL operation is done, and no more data will arrive
    ///     for the specific operation.
    ///     Client -> Server: "indicates that the client has stopped listening and wants to complete the subscription. No further
    ///     events, relevant to the original subscription, should be sent through. Even if the client sent a Complete message for
    ///     a single-result-operation before it resolved, the result should not be sent through once it does."
    ///     Replaces the GQL_STOP in graphql-transport-ws
    ///     id: string : operation ID of the operation that completed
    /// </summary>
    public const string GQL_COMPLETE = "complete"; // Server -> Client and Client -> Server

    /// <summary>
    ///     Client sends this message in order to stop a running GraphQL operation execution (for example: unsubscribe)
    ///     id: string : operation id
    /// </summary>
    public const string GQL_STOP = "stop"; // Client -> Server


    // Additional types for graphql-transport-ws, as described in https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md

    /// <summary>
    ///     Bidirectional. "Useful for detecting failed connections, displaying latency metrics or other types of network probing.
    ///     A Pong must be sent in response from the receiving party as soon as possible. The Ping message can be sent at any time
    ///     within the established socket. The optional payload field can be used to transfer additional details about the ping."
    ///     payload: Object: ping details
    /// </summary>
    public const string GQL_PING = "ping"; // Bidirectional

    /// <summary>
    ///     Bidirectional. "The response to the Ping message. Must be sent as soon as the Ping message is received.
    ///     The Pong message can be sent at any time within the established socket. Furthermore, the Pong message
    ///     may even be sent unsolicited as an unidirectional heartbeat. The optional payload field can be used to
    ///     transfer additional details about the pong."
    ///     payload: Object: pong details
    /// </summary>
    public const string GQL_PONG = "pong"; // Bidirectional

    /// <summary>
    ///     Client-> Server. "Requests an operation specified in the message payload. This message provides a unique
    ///     ID field to connect published messages to the operation requested by this message. If there is already an
    ///     active subscriber for an operation matching the provided ID, regardless of the operation type, the server
    ///     must close the socket immediately with the event 4409: Subscriber for *unique-operation-id* already exists.
    ///     The server needs only keep track of IDs for as long as the subscription is active. Once a client completes
    ///     an operation, it is free to re-use that ID."
    ///     id: string : operation id
    ///     payload: Object:
    ///         operationName : string : subscribe
    ///         query : string : the subscription query
    ///         variables : Dictionary(string, string) : a dictionary with variables and their values
    ///         extensions : Dictionary(string, string) : a dictionary of extensions
    /// </summary>
    public const string GQL_SUBSCRIBE = "subscribe"; // Client -> Server


    /// <summary>
    ///     Server -> Client. "Operation execution result(s) from the source stream created by the binding Subscribe
    ///     message. After all results have been emitted, the Complete message will follow indicating stream
    ///     completion."
    ///     id: string : operation id
    ///     payload: Object: ExecutionResult
    /// </summary>

    public const string GQL_NEXT = "next"; // Server -> Client
    
}
