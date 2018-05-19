using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the result of a subscription query
	/// </summary>
	[Obsolete("EXPERIMENTAL API")]
	public class GraphQLSubscriptionResult : IDisposable {

		public event Action<GraphQLResponse> OnReceive;

		public GraphQLResponse LastResponse { get; }

		private readonly ClientWebSocket clientWebSocket = new ClientWebSocket();

		public async void StartAsync(CancellationToken cancellationToken = default) {
			await this.clientWebSocket.ConnectAsync(new Uri("ws://localhost:5000/"), cancellationToken).ConfigureAwait(false);
			var arraySegment = new ArraySegment<byte>(new byte[1024]);
			while (this.clientWebSocket.State == WebSocketState.Open) {
				var webSocketReceiveResult = await this.clientWebSocket.ReceiveAsync(arraySegment, cancellationToken).ConfigureAwait(false);
				var stringResult = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);
				var graphQLResponse = JsonConvert.DeserializeObject<GraphQLResponse>(stringResult);
				if (graphQLResponse != null) { this.OnReceive.Invoke(graphQLResponse); }
			}
		}

		public void Dispose() => this.clientWebSocket.Dispose();

	}

}
