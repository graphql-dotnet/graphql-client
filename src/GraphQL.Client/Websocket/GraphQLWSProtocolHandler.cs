using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;

internal class GraphQLWSProtocolHandler : IWebsocketProtocolHandler
{
    public string WebsocketProtocol => WebSocketProtocols.GRAPHQL_WS;

    private readonly GraphQLHttpWebSocket _webSocketHandler;
    private readonly GraphQLHttpClient _client;
    private readonly Func<GraphQLWebSocketRequest, Task> _queueWebSocketRequest;
    private readonly Func<GraphQLWebSocketRequest, CancellationToken, Task> _sendWebsocketMessage;

    public GraphQLWSProtocolHandler(
        GraphQLHttpWebSocket webSocketHandler,
        GraphQLHttpClient client,
        Func<GraphQLWebSocketRequest, Task> queueWebSocketRequest,
        Func<GraphQLWebSocketRequest, CancellationToken, Task> sendWebsocketMessage)
    {
        _webSocketHandler = webSocketHandler;
        _client = client;
        _queueWebSocketRequest = queueWebSocketRequest;
        _sendWebsocketMessage = sendWebsocketMessage;
    }

    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionObservable<TResponse>(GraphQLRequest request)
        => Observable.Create<GraphQLResponse<TResponse>>(async observer =>
        {
            Debug.WriteLine($"Create observable thread id: {Thread.CurrentThread.ManagedThreadId}");
            var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client).ConfigureAwait(false);

            var startRequest = new GraphQLWebSocketRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = GraphQLWebSocketMessageType.GQL_START,
                Payload = preprocessedRequest
            };
            var stopRequest = new GraphQLWebSocketRequest
            {
                Id = startRequest.Id,
                Type = GraphQLWebSocketMessageType.GQL_STOP
            };

            var observable = Observable.Create<GraphQLResponse<TResponse>>(o =>
                _webSocketHandler.IncomingMessageStream
                    // ignore null values and messages for other requests
                    .Where(response => response != null && response.Id == startRequest.Id)
                    .Subscribe(response =>
                    {
                        // terminate the sequence when a 'complete' message is received
                        if (response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
                        {
                            Debug.WriteLine($"received 'complete' message on subscription {startRequest.Id}");
                            o.OnCompleted();
                            return;
                        }

                        // post the GraphQLResponse to the stream (even if a GraphQL error occurred)
                        Debug.WriteLine($"received payload on subscription {startRequest.Id} (thread {Thread.CurrentThread.ManagedThreadId})");
                        var typedResponse =
                                    _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLResponse<TResponse>>(
                                        response.MessageBytes);
                        Debug.WriteLine($"payload => {System.Text.Encoding.UTF8.GetString(response.MessageBytes)}");
                        o.OnNext(typedResponse.Payload);

                        // in case of a GraphQL error, terminate the sequence after the response has been posted
                        if (response.Type == GraphQLWebSocketMessageType.GQL_ERROR)
                        {
                            Debug.WriteLine($"terminating subscription {startRequest.Id} because of a GraphQL error");
                            o.OnCompleted();
                        }
                    },
                        e =>
                        {
                            Debug.WriteLine($"response stream for subscription {startRequest.Id} failed: {e}");
                            o.OnError(e);
                        },
                        () =>
                        {
                            Debug.WriteLine($"response stream for subscription {startRequest.Id} completed");
                            o.OnCompleted();
                        })
            );

            try
            {
                // initialize websocket (completes immediately if socket is already open)
                await _webSocketHandler.InitializeWebSocket().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // subscribe observer to failed observable
                return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
            }

            var disposable = new CompositeDisposable(
                observable.Subscribe(observer),
                Disposable.Create(async () =>
                {
                    Debug.WriteLine($"disposing subscription {startRequest.Id}, websocket state is '{_webSocketHandler.WebSocketState}'");
                    // only try to send close request on open websocket
                    if (_webSocketHandler.WebSocketState != WebSocketState.Open)
                        return;

                    try
                    {
                        Debug.WriteLine($"sending stop message on subscription {startRequest.Id}");
                        await _queueWebSocketRequest(stopRequest).ConfigureAwait(false);
                    }
                    // do not break on disposing
                    catch (OperationCanceledException) { }
                })
            );

            Debug.WriteLine($"sending start message on subscription {startRequest.Id}");
            // send subscription request
            try
            {
                await _queueWebSocketRequest(startRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }

            return disposable;
        });

    public IObservable<GraphQLResponse<TResponse>> CreateGraphQLRequestObservable<TResponse>(GraphQLRequest request)
        => Observable.Create<GraphQLResponse<TResponse>>(async observer =>
        {
            var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client).ConfigureAwait(false);
            var websocketRequest = new GraphQLWebSocketRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = GraphQLWebSocketMessageType.GQL_START,
                Payload = preprocessedRequest
            };
            var observable = _webSocketHandler.IncomingMessageStream
                .Where(response => response != null && response.Id == websocketRequest.Id)
                .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
                .Select(response =>
                {
                    Debug.WriteLine($"received response for request {websocketRequest.Id}");
                    var typedResponse =
                        _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLResponse<TResponse>>(
                            response.MessageBytes);
                    return typedResponse.Payload;
                });

            try
            {
                // initialize websocket (completes immediately if socket is already open)
                await _webSocketHandler.InitializeWebSocket().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // subscribe observer to failed observable
                return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
            }

            var disposable = new CompositeDisposable(
                observable.Subscribe(observer)
            );

            Debug.WriteLine($"submitting request {websocketRequest.Id}");
            // send request
            try
            {
                await _queueWebSocketRequest(websocketRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }

            return disposable;
        });


    public async Task InitializeConnectionAsync(IObservable<WebsocketMessageWrapper> incomingMessages,
        CompositeDisposable closeConnectionDisposable)
    {
        var initRequest = new GraphQLWebSocketRequest
        {
            Type = GraphQLWebSocketMessageType.GQL_CONNECTION_INIT,
            Payload = _client.Options.ConfigureWebSocketConnectionInitPayload(_client.Options)
        };

        // setup task to await connection_ack message
        var ackTask = incomingMessages
            .Where(response => response != null)
            .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ACK ||
                                   response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ERROR)
            .LastAsync()
            .ToTask();

        // send connection init
        Debug.WriteLine($"sending connection init message");
        await _sendWebsocketMessage(initRequest, CancellationToken.None).ConfigureAwait(false);
        var response = await ackTask.ConfigureAwait(false);

        if (response.Type != GraphQLWebSocketMessageType.GQL_CONNECTION_ACK)
        {
            string? errorPayload = Encoding.UTF8.GetString(response.MessageBytes);
            Debug.WriteLine($"connection error received: {errorPayload}");
            throw new GraphQLWebsocketConnectionException(errorPayload);
        }

        Debug.WriteLine($"connection acknowledged: {Encoding.UTF8.GetString(response.MessageBytes)}");
    }

    public Task SendCloseConnectionRequestAsync()
        => _sendWebsocketMessage(new GraphQLWebSocketRequest { Type = GraphQLWebSocketMessageType.GQL_CONNECTION_TERMINATE }, CancellationToken.None);

    public IObservable<object?> CreatePongObservable()
        => throw PingPongNotSupportedException;

    public Task SendPingAsync(object? payload)
        => throw PingPongNotSupportedException;

    public Task SendPongAsync(object? payload)
        => throw PingPongNotSupportedException;

    private NotSupportedException PingPongNotSupportedException
        => new("ping/pong is not supported by the \"graphql-ws\" websocket protocol");
}
