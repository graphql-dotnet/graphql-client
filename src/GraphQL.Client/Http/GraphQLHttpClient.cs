using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http.Internal;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Http {

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	public partial class GraphQLHttpClient : IGraphQLClient {

		#region Properties

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public HttpRequestHeaders DefaultRequestHeaders => this.graphQLHttpHandler.HttpClient.DefaultRequestHeaders;

		/// <summary>
		/// The GraphQL EndPoint to be used
		/// </summary>
		public Uri EndPoint {
			get => this.Options.EndPoint;
			set => this.Options.EndPoint = value;
		}

		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLHttpClientOptions Options {
			get => this.graphQLHttpHandler.Options;
			set => this.graphQLHttpHandler.Options = value;
		}

		#endregion

		internal readonly GraphQLHttpHandler graphQLHttpHandler;
		internal readonly GraphQLHttpWebSocket graphQlHttpWebSocket;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLHttpClient(string endPoint) : this(new Uri(endPoint)) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLHttpClient(Uri endPoint) : this(new GraphQLHttpClientOptions { EndPoint = endPoint }) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(string endPoint, GraphQLHttpClientOptions options) : this(new Uri(endPoint), options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(Uri endPoint, GraphQLHttpClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			options.EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(_getWebSocketUri());
		}

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="options">The Options to be used</param>
		public GraphQLHttpClient(GraphQLHttpClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(_getWebSocketUri());
		}

		internal GraphQLHttpClient(GraphQLHttpClientOptions options, HttpClient httpClient) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.graphQLHttpHandler = new GraphQLHttpHandler(options, httpClient);
			this.graphQlHttpWebSocket = new GraphQLHttpWebSocket(_getWebSocketUri());
		}

		public Task<GraphQLResponse> SendQueryAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendQueryAsync(new GraphQLRequest { Query = query }, cancellationToken);

		public Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.PostAsync(request, cancellationToken);

		public Task<GraphQLResponse> SendMutationAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendMutationAsync(new GraphQLRequest { Query = query }, cancellationToken);

		public Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			this.graphQLHttpHandler.PostAsync(request, cancellationToken);

		[Obsolete("EXPERIMENTAL API")]
		public Task<IGraphQLSubscriptionResult> SendSubscribeAsync(string query, CancellationToken cancellationToken = default) =>
			this.SendSubscribeAsync(new GraphQLRequest { Query = query }, cancellationToken);

		[Obsolete("EXPERIMENTAL API")]
		public Task<IGraphQLSubscriptionResult> SendSubscribeAsync(GraphQLRequest request, CancellationToken cancellationToken = default)
		{
			GraphQLHttpSubscriptionResult graphQLSubscriptionResult = _createSubscription(request, cancellationToken);
			return Task.FromResult<IGraphQLSubscriptionResult>(graphQLSubscriptionResult);
		}

		private GraphQLHttpSubscriptionResult _createSubscription(GraphQLRequest request, CancellationToken cancellationToken)
		{
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			var graphQLSubscriptionResult = new GraphQLHttpSubscriptionResult(_getWebSocketUri(), request);
			graphQLSubscriptionResult.StartAsync(cancellationToken);
			return graphQLSubscriptionResult;
		}

		private Uri _getWebSocketUri()
		{
			var webSocketSchema = this.EndPoint.Scheme == "https" ? "wss" : "ws";
			return new Uri($"{webSocketSchema}://{this.EndPoint.Host}:{this.EndPoint.Port}{this.EndPoint.AbsolutePath}");
		}

		/// <inheritdoc />
		[Obsolete("EXPERIMENTAL API")]
		public IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request)
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

					var disposable = new CompositeDisposable(
						Disposable.Create(async () =>
						{
							try
							{
								Debug.WriteLine($"sending close message on subscription {startRequest.Id}");
								await graphQlHttpWebSocket.SendWebSocketRequest(closeRequest).ConfigureAwait(false);
							}
							catch (OperationCanceledException) { }
						}),
						// subscribe to result stream
						graphQlHttpWebSocket.ResponseStream
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
										return Observable.Return(((JObject)response?.Payload)
											?.ToObject<GraphQLResponse>());
								}
							})
							.Subscribe(observer)
					);

					Debug.WriteLine($"sending initial message on subscription {startRequest.Id}");
					// send subscription request
					await graphQlHttpWebSocket.SendWebSocketRequest(startRequest).ConfigureAwait(false);

					return disposable;
				})
			)
			// transform to hot observable and auto-connect
			.Publish().RefCount();
		}

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose()
		{
			this.graphQLHttpHandler.Dispose();
			this.graphQlHttpWebSocket.Dispose();
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}
	}

}
