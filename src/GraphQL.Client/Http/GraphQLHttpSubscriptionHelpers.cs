using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http
{
	public static class GraphQLHttpSubscriptionHelpers
	{
		internal static IObservable<GraphQLResponse> CreateSubscriptionStream(
			GraphQLRequest request,
			GraphQLHttpWebSocket graphQlHttpWebSocket,
			GraphQLHttpClientOptions options,
			CancellationToken cancellationToken = default)
		{
			int connectionAttempt = 0;

			return Observable.Defer(() =>
				Observable.Create<GraphQLResponse>(async observer =>
				{
					var startRequest = new GraphQLWebSocketRequest
					{
						Id = Guid.NewGuid().ToString("N"),
						Type = GQLWebSocketMessageType.GQL_START,
						Payload = request
					};
					var closeRequest = new GraphQLWebSocketRequest
					{
						Id = startRequest.Id,
						Type = GQLWebSocketMessageType.GQL_STOP
					};


					var observable = graphQlHttpWebSocket.ResponseStream
						.Where(response => response.Id == startRequest.Id)
						.SelectMany(response =>
						{
							switch (response.Type)
							{
								case GQLWebSocketMessageType.GQL_COMPLETE:
									Debug.WriteLine(
										$"received 'complete' message on subscription {startRequest.Id}");
									return Observable.Empty<GraphQLResponse>();
								case GQLWebSocketMessageType.GQL_ERROR:
									Debug.WriteLine($"received 'error' message on subscription {startRequest.Id}");
									return Observable.Throw<GraphQLResponse>(
										new GraphQLSubscriptionException(response.Payload));
								default:
									Debug.WriteLine($"received payload on subscription {startRequest.Id}");
									return Observable.Return(((JObject) response?.Payload)
										?.ToObject<GraphQLResponse>());
							}
						});

					Debug.WriteLine($"subscription connection attempt #{connectionAttempt}");
					observable = (++connectionAttempt == 1)
						? observable
						: observable.DelaySubscription(options.BackOffStrategy(connectionAttempt - 1));

					var disposable = new CompositeDisposable(
						Disposable.Create(async () =>
						{
							connectionAttempt = 0;
							try
							{
								Debug.WriteLine($"sending close message on subscription {startRequest.Id}");
								await graphQlHttpWebSocket.SendWebSocketRequest(closeRequest).ConfigureAwait(false);
							}
							catch (OperationCanceledException) { }
						}),
						// subscribe to result stream
						observable.Subscribe(observer)
					);

					Debug.WriteLine($"sending initial message on subscription {startRequest.Id}");
					// send subscription request
					await graphQlHttpWebSocket.SendWebSocketRequest(startRequest).ConfigureAwait(false);

					return disposable;
				}))
				// complete sequence on OperationCanceledException, this is triggered by the cancellation token
				.Catch<GraphQLResponse, OperationCanceledException>(exception =>
					Observable.Empty<GraphQLResponse>())
				// wrap results
				.Select(response => new Tuple<GraphQLResponse, Exception>(response, null))
				// do exception handling
				.Catch<Tuple<GraphQLResponse, Exception>, Exception>(e =>
				{
					try
					{
						// exceptions thrown by the handler will propagate to OnError()
						options.WebSocketExceptionHandler?.Invoke(e);

						// throw exception on the observable to be caught by Retry() or complete sequence if cancellation was requested
						return cancellationToken.IsCancellationRequested
							? Observable.Empty<Tuple<GraphQLResponse, Exception>>()
							: Observable.Throw<Tuple<GraphQLResponse, Exception>>(e);
					}
					catch (Exception exception)
					{
						// wrap all other exceptions to be propagated behind retry
						return Observable.Return(new Tuple<GraphQLResponse, Exception>(null, exception));
					}
				})
				// attempt to recreate the websocket for rethrown exceptions
				.Retry()
				// unwrap and push results or throw wrapped exceptions
				.SelectMany(t =>
				{
					// if the result contains an exception, throw it on the observable
					if (t.Item2 != null)
						return Observable.Throw<GraphQLResponse>(t.Item2);

					connectionAttempt = 1;
					return t.Item1 == null
						? Observable.Empty<GraphQLResponse>()
						: Observable.Return(t.Item1);
				})
				// transform to hot observable and auto-connect
				.Publish().RefCount();
		}
	}
}
