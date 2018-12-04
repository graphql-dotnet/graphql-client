using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http {

	/// <summary>
	/// Represents the result of a subscription query
	/// </summary>
	[Obsolete("EXPERIMENTAL API")]
	public class GraphQLHttpObservableSubscription : IDisposable {

		private readonly ClientWebSocket clientWebSocket = new ClientWebSocket();
		private readonly Uri webSocketUri;
		private readonly GraphQLRequest graphQLRequest;
		private readonly byte[] buffer = new byte[1024 * 1024];
		private readonly ArraySegment<byte> arraySegment;
		private readonly CancellationTokenSource _cancellationTokenSource;

		private GraphQLHttpObservableSubscription(Uri webSocketUri, GraphQLRequest graphQLRequest, CancellationToken cancellationToken = default) {
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_cancellationTokenSource.Token.Register(Dispose);
			this.webSocketUri = webSocketUri;
			this.graphQLRequest = graphQLRequest;
			this.clientWebSocket.Options.AddSubProtocol("graphql-ws");

			arraySegment = new ArraySegment<byte>(buffer);
		}

		public async Task ConnectAsync(CancellationToken token)
		{
			Debug.WriteLine($"opening websocket on subscription {this.GetHashCode()}");
			await clientWebSocket.ConnectAsync(webSocketUri, token).ConfigureAwait(false);
			Debug.WriteLine($"connection established on subscription {this.GetHashCode()}");
		}

		public async Task<GraphQLResponse> ReceiveResultAsync()
		{
			var webSocketReceiveResult = await clientWebSocket.ReceiveAsync(arraySegment, _cancellationTokenSource.Token).ConfigureAwait(false);
			var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
			var webSocketResponse = JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
			switch (webSocketResponse.Type)
			{
				case GQLWebSocketMessageType.GQL_COMPLETE:
					Debug.WriteLine($"received 'complete' message on subscription {this.GetHashCode()}");
					Dispose();
					break;
				case GQLWebSocketMessageType.GQL_ERROR:
					Debug.WriteLine($"received 'error' message on subscription {this.GetHashCode()}");
					throw new GQLSubscriptionException(webSocketResponse.Payload);
				default:
					Debug.WriteLine($"received payload on subscription {this.GetHashCode()}");
					break;
			}

			return ((JObject)webSocketResponse?.Payload).ToObject<GraphQLResponse>();
		}

		public async Task CloseAsync(CancellationToken cancellationToken = default)
		{
			// don't attempt to close the websocket if it is in a failed state
			if (this.clientWebSocket.State != WebSocketState.Open &&
			    this.clientWebSocket.State != WebSocketState.CloseReceived &&
			    this.clientWebSocket.State != WebSocketState.CloseSent)
				return;

			Debug.WriteLine($"closing websocket on subscription {this.GetHashCode()}");
			if (this.clientWebSocket.State == WebSocketState.Open) {
				await SendCloseMessageAsync(cancellationToken).ConfigureAwait(false);
			}
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken).ConfigureAwait(false);
		}

		private Task SendInitialMessageAsync(CancellationToken cancellationToken = default)
		{
			Debug.WriteLine($"sending initial message on subscription {this.GetHashCode()}");
			var webSocketRequest = new GraphQLWebSocketRequest
			{
				Id = "1",
				Type = GQLWebSocketMessageType.GQL_START,
				Payload = this.graphQLRequest
			};
			return this.SendGraphQLSubscriptionRequest(webSocketRequest, cancellationToken);
		}

		private Task SendCloseMessageAsync(CancellationToken cancellationToken = default)
		{
			Debug.WriteLine($"sending close message on subscription {this.GetHashCode()}");
			var webSocketRequest = new GraphQLWebSocketRequest
			{
				Id = "1",
				Type = GQLWebSocketMessageType.GQL_STOP,
				Payload = this.graphQLRequest
			};
			return this.SendGraphQLSubscriptionRequest(webSocketRequest, cancellationToken);
		}

		private Task SendGraphQLSubscriptionRequest(GraphQLWebSocketRequest graphQlWebSocketRequest, CancellationToken cancellationToken = default)
		{
			var webSocketRequestString = JsonConvert.SerializeObject(graphQlWebSocketRequest);
			var arraySegmentWebSocketRequest = new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString));
			return this.clientWebSocket.SendAsync(arraySegmentWebSocketRequest, WebSocketMessageType.Text, true, cancellationToken);
		}

		#region IDisposable
		public Task Disposed { get; private set; }
		private object _disposedLocker = new object();
		public void Dispose()
		{
			// Async disposal as recommended by Stephen Cleary (https://blog.stephencleary.com/2013/03/async-oop-6-disposal.html)
			lock (_disposedLocker)
			{
				if (Disposed == null) Disposed = DisposeAsync();
			}
		}

		private async Task DisposeAsync()
		{
			Debug.WriteLine($"disposing subscription {this.GetHashCode()}...");
			if(!_cancellationTokenSource.IsCancellationRequested)
				_cancellationTokenSource.Cancel();
			await CloseAsync().ConfigureAwait(false);
			clientWebSocket?.Dispose();
			_cancellationTokenSource.Dispose();
			Debug.WriteLine($"subscription {this.GetHashCode()} disposed");
		}

		#endregion

		#region Static Factories

		internal static IObservable<GraphQLResponse> GetSubscriptionStream(
			GraphQLHttpWebSocket webSocket,
			GraphQLRequest graphQLRequest)
		{
			return Observable.Defer(() =>
				Observable.Create<GraphQLResponse>(async observer =>
				{
					var startRequest = new GraphQLWebSocketRequest
					{
						Id = Guid.NewGuid().ToString("N"),
						Type = GQLWebSocketMessageType.GQL_START,
						Payload = graphQLRequest
					};
					var closeRequest = new GraphQLWebSocketRequest
					{
						Id = startRequest.Id,
						Type = GQLWebSocketMessageType.GQL_STOP
					};

					var disposable = new CompositeDisposable(
						Disposable.Create(async () =>
						{
							Debug.WriteLine($"sending close message on subscription {startRequest.Id}");
							await webSocket.SendWebSocketRequest(closeRequest).ConfigureAwait(false);
						}),
						// subscribe to result stream
						webSocket.ResponseStream
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
											new GQLSubscriptionException(response.Payload));
									default:
										Debug.WriteLine($"received payload on subscription {startRequest.Id}");
										return Observable.Return(((JObject) response?.Payload)
											?.ToObject<GraphQLResponse>());
								}
							})
							.Subscribe(observer)
					);

					Debug.WriteLine($"sending initial message on subscription {startRequest.Id}");
					// send subscription request
					await webSocket.SendWebSocketRequest(startRequest).ConfigureAwait(false);

					return disposable;
				})
			)
			// transform to hot observable and auto-connect
			.Publish().RefCount();
		}

		/// <summary>
		/// The back-off strategy for automatic reconnects. Calculates the delay before the next connection attempt is made.<br/>
		/// default formula: min(n, 5) * 1,5 * random(0.0, 1.0)
		/// </summary>
		public static Func<int, TimeSpan> BackOffStrategy = n =>
		{
			var rnd = new Random();
			return TimeSpan.FromSeconds(Math.Min(n, 5) * 1.5 + rnd.NextDouble());
		};

		private static Task<GraphQLHttpObservableSubscription> CreateSubscription(Uri webSocketUri, GraphQLRequest graphQLRequest, CancellationToken cancellationToken = default)
		{
			var subscription = new GraphQLHttpObservableSubscription(webSocketUri, graphQLRequest, cancellationToken);
			return Task.FromResult(subscription);
		}

		private static async Task<IObservable<GraphQLResponse>> InitializeSubscription(GraphQLHttpObservableSubscription observableSubscription, CancellationToken cancelToken)
		{
			await observableSubscription.ConnectAsync(cancelToken).ConfigureAwait(false);
			await observableSubscription.SendInitialMessageAsync(cancelToken).ConfigureAwait(false);
			return Observable.Defer(() => observableSubscription.ReceiveResultAsync().ToObservable()).Repeat();
		}

		#endregion

		[Serializable]
		public class GQLSubscriptionException : Exception
		{
			//
			// For guidelines regarding the creation of new exception types, see
			//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
			// and
			//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
			//

			public GQLSubscriptionException()
			{
			}

			public GQLSubscriptionException(object error) : base(error.ToString())
			{
			}

			protected GQLSubscriptionException(
				SerializationInfo info,
				StreamingContext context) : base(info, context)
			{
			}
		}
	}
}
