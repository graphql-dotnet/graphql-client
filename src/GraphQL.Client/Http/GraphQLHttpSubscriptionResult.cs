using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client.Http {

	/// <summary>
	/// Represents the result of a subscription query
	/// </summary>
	[Obsolete("EXPERIMENTAL API")]
	public class GraphQLHttpSubscriptionResult : IGraphQLSubscriptionResult {

		public event Action<GraphQLResponse> OnReceive;

		public GraphQLResponse LastResponse { get; private set; }

		private readonly ClientWebSocket clientWebSocket = new ClientWebSocket();
		private readonly Uri webSocketUri;
		private readonly GraphQLRequest graphQLRequest;
		private readonly byte[] buffer = new byte[1024 * 1024];

		internal GraphQLHttpSubscriptionResult(Uri webSocketUri, GraphQLRequest graphQLRequest) {
			this.webSocketUri = webSocketUri;
			this.graphQLRequest = graphQLRequest;
			this.clientWebSocket.Options.AddSubProtocol("graphql-ws");
		}

		public async void StartAsync(CancellationToken cancellationToken = default) {
			await this.clientWebSocket.ConnectAsync(this.webSocketUri, cancellationToken).ConfigureAwait(false);
			if (this.clientWebSocket.State == WebSocketState.Open) {
				var arraySegment = new ArraySegment<byte>(this.buffer);
				await this.SendInitialMessageAsync(cancellationToken).ConfigureAwait(false);
				while (this.clientWebSocket.State == WebSocketState.Open) {
					var webSocketReceiveResult = await this.clientWebSocket.ReceiveAsync(arraySegment, cancellationToken);
					var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
					var webSocketResponse = JsonConvert.DeserializeObject<GraphQLWebSocketResponse>(stringResult);
					if (webSocketResponse != null)
					{
						var response = (GraphQLResponse) webSocketResponse.Payload;
						this.LastResponse = response;
						this.OnReceive?.Invoke(response);
					}
				}
			}
		}

		public async Task StopAsync(CancellationToken cancellationToken = default) {
			if (this.clientWebSocket.State == WebSocketState.Open) {
				await this.SendCloseMessageAsync(cancellationToken);
			}
			await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
		}

		public void Dispose() {
			this.StopAsync().Wait();
			this.clientWebSocket.Dispose();
		}

		private Task SendInitialMessageAsync(CancellationToken cancellationToken = default) {
			var webSocketRequest = new GraphQLWebSocketRequest {
				Id = "1",
				Type = GQLWebSocketMessageType.GQL_START,
				Payload = this.graphQLRequest
			};
			return this.SendGraphQLSubscriptionRequest(webSocketRequest, cancellationToken);
		}

		private Task SendCloseMessageAsync(CancellationToken cancellationToken = default) {
			var webSocketRequest = new GraphQLWebSocketRequest {
				Id = "1",
				Type = GQLWebSocketMessageType.GQL_STOP,
				Payload = this.graphQLRequest
			};
			return this.SendGraphQLSubscriptionRequest(webSocketRequest);
		}

		private Task SendGraphQLSubscriptionRequest(GraphQLWebSocketRequest graphQlWebSocketRequest, CancellationToken cancellationToken = default) {
			var webSocketRequestString = JsonConvert.SerializeObject(graphQlWebSocketRequest);
			var arraySegmentWebSocketRequest = new ArraySegment<byte>(Encoding.UTF8.GetBytes(webSocketRequestString));
			return this.clientWebSocket.SendAsync(arraySegmentWebSocketRequest, WebSocketMessageType.Text, true, cancellationToken);
		}
	}

}
