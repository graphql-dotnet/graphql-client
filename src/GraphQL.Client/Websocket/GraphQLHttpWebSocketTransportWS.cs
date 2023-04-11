using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;

//Implements communications using the WebSocket sub-protocol used by
//[GraphQL over WebSocket Protocol](https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md).
internal class GraphQLHttpWebSocketTransportWS : BaseGraphQLHttpWebSocket
{
    public override string WebsocketProtocol => WebSocketProtocols.GRAPHQL_TRANSPORT_WS;

    public GraphQLHttpWebSocketTransportWS(Uri webSocketUri, GraphQLHttpClient client) : base(webSocketUri, client)
    { }

    public override Task<GraphQLResponse<TResponse>> SendRequest<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default) =>
        Observable.Create<GraphQLResponse<TResponse>>(async observer =>
        {
            var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client).ConfigureAwait(false);
            var websocketRequest = new GraphQLWebSocketRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = GraphQLWebSocketMessageType.GQL_SUBSCRIBE,
                Payload = preprocessedRequest
            };
            var observable = IncomingMessageStream
                .Where(response => response != null && response.Id == websocketRequest.Id)
                .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
                .Select(response =>
                {
                    Debug.WriteLine($"received response for request {websocketRequest.Id}");
                    switch (response.Type)
                    {
                        case GraphQLWebSocketMessageType.GQL_NEXT:
                            var typedResponse = _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLResponse<TResponse>>(response.MessageBytes);
                            return typedResponse.Payload;
                        case GraphQLWebSocketMessageType.GQL_ERROR:
                            // the payload only consists of the error array
                            var errorResponse = _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLError[]>(response.MessageBytes);
                            return new GraphQLResponse<TResponse> { Errors = errorResponse.Payload };
                    }
                    return null;
                });

            try
            {
                // initialize websocket (completes immediately if socket is already open)
                await InitializeWebSocket().ConfigureAwait(false);
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
                await QueueWebSocketRequest(websocketRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }

            return disposable;
        })
            // complete sequence on OperationCanceledException, this is triggered by the cancellation token
            .Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
                Observable.Empty<GraphQLResponse<TResponse>>())
            .FirstAsync()
            .ToTask(cancellationToken);

    /// <summary>
    /// Create a new subscription stream using the graphql-transport-ws subprotocol 
    /// </summary>
    /// <typeparam name="TResponse">the response type</typeparam>
    /// <param name="request">the <see cref="GraphQLRequest"/> to start the subscription</param>
    /// <param name="exceptionHandler">Optional: exception handler for handling exceptions within the receive pipeline</param>
    /// <returns>a <see cref="IObservable{TResponse}"/> which represents the subscription</returns>
    public override IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception>? exceptionHandler = null) =>
        Observable.Defer(() =>
                Observable.Create<GraphQLResponse<TResponse>>(async observer =>
                {
                    Debug.WriteLine($"Create observable thread id: {Thread.CurrentThread.ManagedThreadId}");
                    var preprocessedRequest = await _client.Options.PreprocessRequest(request, _client).ConfigureAwait(false);


                    var startRequest = new GraphQLWebSocketRequest
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Type = GraphQLWebSocketMessageType.GQL_SUBSCRIBE,
                        Payload = preprocessedRequest
                    };


                    var stopRequest = new GraphQLWebSocketRequest
                    {
                        Id = startRequest.Id,
                        Type = GraphQLWebSocketMessageType.GQL_COMPLETE
                    };

                    var observable = Observable.Create<GraphQLResponse<TResponse>>(o =>
                        IncomingMessageStream
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

                                    if (!(response.Type == GraphQLWebSocketMessageType.GQL_NEXT || response.Type == GraphQLWebSocketMessageType.GQL_ERROR))
                                        throw new WebSocketException("Did not receive 'next' nor 'error'");

                                    switch (response.Type)
                                    {
                                        case GraphQLWebSocketMessageType.GQL_NEXT:
                                            // post the GraphQLResponse to the stream (even if a GraphQL error occurred)
                                            Debug.WriteLine($"received payload on subscription {startRequest.Id} (thread {Thread.CurrentThread.ManagedThreadId})");
                                            var typedResponse = _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLResponse<TResponse>>(response.MessageBytes);
                                            Debug.WriteLine($"payload => {Encoding.UTF8.GetString(response.MessageBytes)}");
                                            o.OnNext(typedResponse.Payload);
                                            break;
                                        case GraphQLWebSocketMessageType.GQL_ERROR:
                                            // the payload only consists of the error array
                                            Debug.WriteLine($"received error on subscription {startRequest.Id} (thread {Thread.CurrentThread.ManagedThreadId})");
                                            Debug.WriteLine($"payload => {Encoding.UTF8.GetString(response.MessageBytes)}");
                                            var errorResponse = _client.JsonSerializer.DeserializeToWebsocketResponse<GraphQLError[]>(response.MessageBytes);
                                            o.OnNext(new GraphQLResponse<TResponse> { Errors = errorResponse.Payload });
                                            // in case of a GraphQL error, terminate the sequence after the response has been posted
                                            Debug.WriteLine($"terminating subscription {startRequest.Id} because of a GraphQL error");
                                            o.OnCompleted();
                                            break;
                                        default:
                                            // If the message type was not 'complete', then it must only be 'next' or 'error'
                                            throw new WebSocketException("Did not receive 'next' nor 'error'");
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
                        await InitializeWebSocket().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        // subscribe observer to failed observable
                        return (CompositeDisposable)Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
                    }

                    var disposable = new CompositeDisposable(
                        observable.Subscribe(observer),
                        Disposable.Create(async () =>
                        {
                            Debug.WriteLine($"disposing subscription {startRequest.Id}, websocket state is '{WebSocketState}'");
                            // only try to send close request on open websocket
                            if (WebSocketState != WebSocketState.Open)
                                return;

                            try
                            {
                                Debug.WriteLine($"sending stop message on subscription {startRequest.Id}");
                                await QueueWebSocketRequest(stopRequest).ConfigureAwait(false);
                            }
                            // do not break on disposing
                            catch (OperationCanceledException) { }
                        })
                    );

                    Debug.WriteLine($"sending start message on subscription {startRequest.Id}");
                    // send subscription request
                    try
                    {
                        await QueueWebSocketRequest(startRequest).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }

                    return disposable;
                }))
            // complete sequence on OperationCanceledException, this is triggered by the cancellation token
            .Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
                Observable.Empty<GraphQLResponse<TResponse>>())
            // wrap results
            .Select(response => new Tuple<GraphQLResponse<TResponse>, Exception>(response, null))
            // do exception handling
            .Catch<Tuple<GraphQLResponse<TResponse>, Exception>, Exception>(e =>
            {
                try
                {
                    if (exceptionHandler == null)
                    {
                        // if the external handler is not set, propagate all exceptions except WebSocketExceptions
                        // this will ensure that the client tries to re-establish subscriptions on connection loss
                        if (!(e is WebSocketException))
                            throw e;
                    }
                    else
                    {
                        // exceptions thrown by the handler will propagate to OnError()
                        exceptionHandler?.Invoke(e);
                    }

                    // throw exception on the observable to be caught by Retry() or complete sequence if cancellation was requested
                    if (_internalCancellationToken.IsCancellationRequested)
                        return Observable.Empty<Tuple<GraphQLResponse<TResponse>, Exception>>();
                    else
                    {
                        Debug.WriteLine($"Catch handler thread id: {Thread.CurrentThread.ManagedThreadId}");
                        return Observable.Throw<Tuple<GraphQLResponse<TResponse>, Exception>>(e);
                    }
                }
                catch (Exception exception)
                {
                    // wrap all other exceptions to be propagated behind retry
                    return Observable.Return(new Tuple<GraphQLResponse<TResponse>, Exception>(null, exception));
                }
            })
            // attempt to recreate the websocket for rethrown exceptions
            .Retry()
            // unwrap and push results or throw wrapped exceptions
            .SelectMany(t =>
            {
                // if the result contains an exception, throw it on the observable
                if (t.Item2 != null)
                {
                    Debug.WriteLine($"unwrap exception thread id: {Thread.CurrentThread.ManagedThreadId} => {t.Item2}");
                    return Observable.Throw<GraphQLResponse<TResponse>>(t.Item2);
                }
                if (t.Item1 == null)
                {
                    Debug.WriteLine($"empty item thread id: {Thread.CurrentThread.ManagedThreadId}");
                    return Observable.Empty<GraphQLResponse<TResponse>>();
                }
                return Observable.Return(t.Item1);
            });

    protected override async Task CloseAsync()
    {
        if (_clientWebSocket == null)
            return;

        // don't attempt to close the websocket if it is in a failed state
        if (_clientWebSocket.State != WebSocketState.Open &&
            _clientWebSocket.State != WebSocketState.CloseReceived &&
            _clientWebSocket.State != WebSocketState.CloseSent)
        {
            Debug.WriteLine($"websocket {_clientWebSocket.GetHashCode()} state = {_clientWebSocket.State}");
            return;
        }

        Debug.WriteLine($"send \"connection_terminate\" message");
        await SendWebSocketMessageAsync(new GraphQLWebSocketRequest { Type = GraphQLWebSocketMessageType.GQL_COMPLETE }).ConfigureAwait(false);

        Debug.WriteLine($"closing websocket {_clientWebSocket.GetHashCode()}");
        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).ConfigureAwait(false);
        _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);

    }

    protected override async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            //Client sends a WebSocket handshake request with the sub-protocol: graphql-transport-ws
            await BackOff().ConfigureAwait(false);
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connecting);
            Debug.WriteLine($"opening websocket {_clientWebSocket.GetHashCode()} (thread {Thread.CurrentThread.ManagedThreadId})");
            await _clientWebSocket.ConnectAsync(_webSocketUri, token).ConfigureAwait(false);
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Connected);
            Debug.WriteLine($"connection established on websocket {_clientWebSocket.GetHashCode()}, invoking Options.OnWebsocketConnected()");
            await (Options.OnWebsocketConnected?.Invoke(_client) ?? Task.CompletedTask).ConfigureAwait(false);
            Debug.WriteLine($"invoking Options.OnWebsocketConnected() on websocket {_clientWebSocket.GetHashCode()}");
            _connectionAttempt = 1;

            // Client immediately dispatches a ConnectionInit message optionally providing a payload as agreed with the server

            // create receiving observable
            _incomingMessages = Observable
                .Defer(() => GetReceiveTask().ToObservable())
                .Repeat()
                // complete sequence on OperationCanceledException, this is triggered by the cancellation token on disposal
                .Catch<WebsocketMessageWrapper, OperationCanceledException>(exception => Observable.Empty<WebsocketMessageWrapper>())
                .Publish();

            // subscribe maintenance
            var maintenanceSubscription = _incomingMessages.Subscribe(_ => { }, ex =>
            {
                Debug.WriteLine($"incoming message stream {_incomingMessages.GetHashCode()} received an error: {ex}");
                _exceptionSubject.OnNext(ex);
                _incomingMessagesConnection?.Dispose();
                _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
            },
                () =>
                {
                    Debug.WriteLine($"incoming message stream {_incomingMessages.GetHashCode()} completed");
                    _incomingMessagesConnection?.Dispose();
                    _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
                });


            // connect observable
            var connection = _incomingMessages.Connect();
            Debug.WriteLine($"new incoming message stream {_incomingMessages.GetHashCode()} created");

            var pingPongSubscription = new CompositeDisposable();
            _incomingMessagesConnection = new CompositeDisposable(connection, maintenanceSubscription, pingPongSubscription);

            var payload = Options.ConfigureWebSocketConnectionInitPayload(Options);

            GraphQLWebSocketRequest initRequest;

            if (payload != null)
            {
                initRequest = new GraphQLWebSocketRequest
                {
                    Type = GraphQLWebSocketMessageType.GQL_CONNECTION_INIT,
                    Payload = payload
                };
            }
            else
            {
                initRequest = new GraphQLWebSocketRequest
                {
                    Type = GraphQLWebSocketMessageType.GQL_CONNECTION_INIT,
                };

            }


            // setup task to await connection_ack message
            var ackTask = _incomingMessages
                .Where(response => response != null)
                .TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ACK ||
                                       response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ERROR)
                .LastAsync()
                .ToTask();

            // send connection init
            Debug.WriteLine($"sending connection init message");
            await SendWebSocketMessageAsync(initRequest).ConfigureAwait(false);
            var response = await ackTask.ConfigureAwait(false);

            if (response.Type == GraphQLWebSocketMessageType.GQL_CONNECTION_ACK)
                Debug.WriteLine($"connection acknowledged: {Encoding.UTF8.GetString(response.MessageBytes)}");
            else
            {
                var errorPayload = Encoding.UTF8.GetString(response.MessageBytes);
                Debug.WriteLine($"connection error received: {errorPayload}");
                throw new GraphQLWebsocketConnectionException(errorPayload);
            }

            pingPongSubscription.Add(_incomingMessages
                .Where(msg => msg != null && msg.Type == GraphQLWebSocketMessageType.GQL_PING)
                .SelectMany(msg => Observable.FromAsync(() => RespondWithPongAsync(msg)))
                .Subscribe());
        }
        catch (Exception e)
        {
            Debug.WriteLine($"failed to establish websocket connection");
            _stateSubject.OnNext(GraphQLWebsocketConnectionState.Disconnected);
            _exceptionSubject.OnNext(e);
            throw;
        }
    }

    private async Task RespondWithPongAsync(WebsocketMessageWrapper pingMessage)
    {
        // respond with a PONG when a ping is received
        if (pingMessage.Type == GraphQLWebSocketMessageType.GQL_PING)
        {
            object? payload = null;
            try
            {
                // try to deserialize response to put it back in the pong request
                var responseObject = _client.JsonSerializer.DeserializeToWebsocketResponse<object?>(pingMessage.MessageBytes);
                payload = responseObject.Payload;
            }
            catch (Exception)
            {
                // ignore exception
            }
            Debug.WriteLine($"ping received, responding with pong");
            var pongRequest = new GraphQLWebSocketRequest
            {
                Type = GraphQLWebSocketMessageType.GQL_PONG,
                Payload = payload
            };

            await SendWebSocketMessageAsync(pongRequest).ConfigureAwait(false);
        }
    }
}
