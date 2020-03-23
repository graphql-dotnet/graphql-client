namespace GraphQL.Client.Abstractions.Websocket
{
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
        ///     Client sends this message in order to stop a running GraphQL operation execution (for example: unsubscribe)
        ///     id: string : operation id
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
        ///     Server sends this message to indicate that a GraphQL operation is done, and no more data will arrive for the
        ///     specific operation.
        ///     id: string : operation ID of the operation that completed
        /// </summary>
        public const string GQL_COMPLETE = "complete"; // Server -> Client

        /// <summary>
        ///     Client sends this message in order to stop a running GraphQL operation execution (for example: unsubscribe)
        ///     id: string : operation id
        /// </summary>
        public const string GQL_STOP = "stop"; // Client -> Server

    }
}
