using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http
{
	public static class GraphQLHttpSubscriptionHelpers
	{
		internal static IObservable<GraphQLResponse> CreateSubscriptionStream(
			this GraphQLHttpWebSocket graphQlHttpWebSocket,
			GraphQLRequest request,
			GraphQLHttpClientOptions options,
			Action<Exception> exceptionHandler = null,
			CancellationToken cancellationToken = default)
		{
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
						.Where(response => {
							return response != null && response.Id == startRequest.Id;
						})
						.SelectMany(response =>
						{
							switch (response.Type)
							{
								case GQLWebSocketMessageType.GQL_COMPLETE:
									Debug.WriteLine($"received 'complete' message on subscription {startRequest.Id}");
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

					try
					{
						// intialize websocket (completes immediately if socket is already open)
						await graphQlHttpWebSocket.InitializeWebSocket().ConfigureAwait(false);
					}
					catch (Exception e)
					{
						// subscribe observer to failed observable
						return Observable.Throw<GraphQLResponse>(e).Subscribe(observer);
					}

					var disposable = new CompositeDisposable(
						observable.Subscribe(observer),
						Disposable.Create(async () =>
						{
							// only try to send close request on open websocket
							if (graphQlHttpWebSocket.WebSocketState != WebSocketState.Open) return;

							try
							{
								Debug.WriteLine($"sending close message on subscription {startRequest.Id}");
								await graphQlHttpWebSocket.SendWebSocketRequest(closeRequest).ConfigureAwait(false);
							}
							// do not break on disposing
							catch (OperationCanceledException) { }
						})
					);

					Debug.WriteLine($"sending initial message on subscription {startRequest.Id}");
					// send subscription request
					try
					{
						await graphQlHttpWebSocket.SendWebSocketRequest(startRequest).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}

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
						if (exceptionHandler == null) {
							// if the external handler is not set, propagate all exceptions except WebSocketExceptions
							// this will ensure that the client tries to re-establish subscriptions on connection loss
							if (!(e is WebSocketException)) throw e;
						}
						else
						{
							// exceptions thrown by the handler will propagate to OnError()
							exceptionHandler?.Invoke(e);
						}

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

					return t.Item1 == null
						? Observable.Empty<GraphQLResponse>()
						: Observable.Return(t.Item1);
				})
				// transform to hot observable and auto-connect
				.Publish().RefCount();
		}

		internal static async Task<GraphQLResponse> Request(
			this GraphQLHttpWebSocket graphQlHttpWebSocket,
			GraphQLRequest request,
			CancellationToken cancellationToken = default)
		{
			return await Observable.Create<GraphQLResponse>(async observer =>
			{
				var websocketRequest = new GraphQLWebSocketRequest
				{
					Id = Guid.NewGuid().ToString("N"),
					Type = GQLWebSocketMessageType.GQL_START,
					Payload = request
				};
				var observable = graphQlHttpWebSocket.ResponseStream
					.Where(response => {
						return response != null && response.Id == websocketRequest.Id;
					})
					.SelectMany(response =>
					{
						switch (response.Type)
						{
							case GQLWebSocketMessageType.GQL_COMPLETE:
								Debug.WriteLine($"received 'complete' message on request {websocketRequest.Id}");
								return Observable.Empty<GraphQLResponse>();
							default:
								Debug.WriteLine($"received response for request {websocketRequest.Id}");
								return Observable.Return(((JObject)response?.Payload)
									?.ToObject<GraphQLResponse>());
						}
					});

				try
				{
					// intialize websocket (completes immediately if socket is already open)
					await graphQlHttpWebSocket.InitializeWebSocket().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					// subscribe observer to failed observable
					return Observable.Throw<GraphQLResponse>(e).Subscribe(observer);
				}

				var disposable = new CompositeDisposable(
					observable.Subscribe(observer)
				);

				Debug.WriteLine($"submitting request {websocketRequest.Id}");
				// send request
				try
				{
					await graphQlHttpWebSocket.SendWebSocketRequest(websocketRequest).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}

				return disposable;
			})
			// complete sequence on OperationCanceledException, this is triggered by the cancellation token
			.Catch<GraphQLResponse, OperationCanceledException>(exception =>
				Observable.Empty<GraphQLResponse>())
			.FirstOrDefaultAsync();
		}
	}
}
