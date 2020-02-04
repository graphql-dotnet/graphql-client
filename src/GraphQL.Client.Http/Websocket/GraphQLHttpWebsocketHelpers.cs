using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http.Websocket {
	public static class GraphQLHttpWebsocketHelpers {
		internal static IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(
			this GraphQLHttpWebSocket graphQlHttpWebSocket,
			GraphQLRequest request,
			GraphQLHttpClientOptions options,
			Action<Exception> exceptionHandler = null,
			CancellationToken cancellationToken = default) {
			return Observable.Defer(() =>
				Observable.Create<GraphQLResponse<TResponse>>(async observer => {
					var startRequest = new GraphQLWebSocketRequest {
						Id = Guid.NewGuid().ToString("N"),
						Type = GraphQLWebSocketMessageType.GQL_START,
						Payload = request
					};
					var closeRequest = new GraphQLWebSocketRequest {
						Id = startRequest.Id,
						Type = GraphQLWebSocketMessageType.GQL_STOP
					};

					var observable = Observable.Create<GraphQLResponse<TResponse>>(o =>
						graphQlHttpWebSocket.ResponseStream
							// ignore null values and messages for other requests
							.Where(response => response != null && response.Id == startRequest.Id)
							.Subscribe(response => {
								// terminate the sequence when a 'complete' message is received
								if (response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE) {
									Debug.WriteLine($"received 'complete' message on subscription {startRequest.Id}");
									o.OnCompleted();
									return;
								}

								// post the GraphQLResponse to the stream (even if a GraphQL error occurred)
								Debug.WriteLine($"received payload on subscription {startRequest.Id}");
								var typedResponse =
									JsonSerializer.Deserialize<GraphQLWebSocketResponse<TResponse>>(response.MessageBytes,
										options.JsonSerializerOptions);
								o.OnNext(typedResponse.Payload);

								// in case of a GraphQL error, terminate the sequence after the response has been posted
								if (response.Type == GraphQLWebSocketMessageType.GQL_ERROR) {
									Debug.WriteLine($"terminating subscription {startRequest.Id} because of a GraphQL error");
									o.OnCompleted();
								}
							},
							o.OnError,
							o.OnCompleted)
					);

					try {
						// initialize websocket (completes immediately if socket is already open)
						await graphQlHttpWebSocket.InitializeWebSocket().ConfigureAwait(false);
					}
					catch (Exception e) {
						// subscribe observer to failed observable
						return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
					}

					var disposable = new CompositeDisposable(
						observable.Subscribe(observer),
						Disposable.Create(async () => {
							// only try to send close request on open websocket
							if (graphQlHttpWebSocket.WebSocketState != WebSocketState.Open) return;

							try {
								Debug.WriteLine($"sending close message on subscription {startRequest.Id}");
								await graphQlHttpWebSocket.SendWebSocketRequest(closeRequest).ConfigureAwait(false);
							}
							// do not break on disposing
							catch (OperationCanceledException) { }
						})
					);

					Debug.WriteLine($"sending initial message on subscription {startRequest.Id}");
					// send subscription request
					try {
						await graphQlHttpWebSocket.SendWebSocketRequest(startRequest).ConfigureAwait(false);
					}
					catch (Exception e) {
						Console.WriteLine(e);
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
				.Catch<Tuple<GraphQLResponse<TResponse>, Exception>, Exception>(e => {
					try {
						if (exceptionHandler == null) {
							// if the external handler is not set, propagate all exceptions except WebSocketExceptions
							// this will ensure that the client tries to re-establish subscriptions on connection loss
							if (!(e is WebSocketException)) throw e;
						}
						else {
							// exceptions thrown by the handler will propagate to OnError()
							exceptionHandler?.Invoke(e);
						}

						// throw exception on the observable to be caught by Retry() or complete sequence if cancellation was requested
						return cancellationToken.IsCancellationRequested
							? Observable.Empty<Tuple<GraphQLResponse<TResponse>, Exception>>()
							: Observable.Throw<Tuple<GraphQLResponse<TResponse>, Exception>>(e);
					}
					catch (Exception exception) {
						// wrap all other exceptions to be propagated behind retry
						return Observable.Return(new Tuple<GraphQLResponse<TResponse>, Exception>(null, exception));
					}
				})
				// attempt to recreate the websocket for rethrown exceptions
				.Retry()
				// unwrap and push results or throw wrapped exceptions
				.SelectMany(t => {
					// if the result contains an exception, throw it on the observable
					if (t.Item2 != null)
						return Observable.Throw<GraphQLResponse<TResponse>>(t.Item2);

					return t.Item1 == null
						? Observable.Empty<GraphQLResponse<TResponse>>()
						: Observable.Return(t.Item1);
				})
				// transform to hot observable and auto-connect
				.Publish().RefCount();
		}

		internal static Task<GraphQLResponse<TResponse>> Request<TResponse>(
			this GraphQLHttpWebSocket graphQlHttpWebSocket,
			GraphQLRequest request,
			GraphQLHttpClientOptions options,
			CancellationToken cancellationToken = default) {
			return Observable.Create<GraphQLResponse<TResponse>>(async observer => {
				var websocketRequest = new GraphQLWebSocketRequest {
					Id = Guid.NewGuid().ToString("N"),
					Type = GraphQLWebSocketMessageType.GQL_START,
					Payload = request
				};
				var observable = graphQlHttpWebSocket.ResponseStream
					.Where(response => response != null && response.Id == websocketRequest.Id)
					.TakeUntil(response => response.Type == GraphQLWebSocketMessageType.GQL_COMPLETE)
					.Select(response => {
						Debug.WriteLine($"received response for request {websocketRequest.Id}");
						var typedResponse =
							JsonSerializer.Deserialize<GraphQLWebSocketResponse<TResponse>>(response.MessageBytes,
								options.JsonSerializerOptions);
						return typedResponse.Payload;
					});

				try {
					// intialize websocket (completes immediately if socket is already open)
					await graphQlHttpWebSocket.InitializeWebSocket().ConfigureAwait(false);
				}
				catch (Exception e) {
					// subscribe observer to failed observable
					return Observable.Throw<GraphQLResponse<TResponse>>(e).Subscribe(observer);
				}

				var disposable = new CompositeDisposable(
					observable.Subscribe(observer)
				);

				Debug.WriteLine($"submitting request {websocketRequest.Id}");
				// send request
				try {
					await graphQlHttpWebSocket.SendWebSocketRequest(websocketRequest).ConfigureAwait(false);
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}

				return disposable;
			})
			// complete sequence on OperationCanceledException, this is triggered by the cancellation token
			.Catch<GraphQLResponse<TResponse>, OperationCanceledException>(exception =>
				Observable.Empty<GraphQLResponse<TResponse>>())
			.FirstOrDefaultAsync()
			.ToTask(cancellationToken);
		}
	}
}
