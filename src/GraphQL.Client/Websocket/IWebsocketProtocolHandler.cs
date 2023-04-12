using System.Reactive.Disposables;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;

public interface IWebsocketProtocolHandler
{
    string WebsocketProtocol { get; }

    IObservable<GraphQLResponse<TResponse>> CreateSubscriptionObservable<TResponse>(GraphQLRequest request);

    IObservable<GraphQLResponse<TResponse>> CreateGraphQLRequestObservable<TResponse>(GraphQLRequest request);

    IObservable<object?> CreatePongObservable();

    Task InitializeConnectionAsync(IObservable<WebsocketMessageWrapper> incomingMessages, CompositeDisposable closeConnectionDisposable);

    Task SendCloseConnectionRequestAsync();

    Task SendPingAsync(object? payload);

    Task SendPongAsync(object? payload);
}
