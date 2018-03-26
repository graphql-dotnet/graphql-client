using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client.Experimental {

	/// <summary>
	/// Represents the result of a Subscription Query
	/// </summary>
	[Obsolete("EXPERIMENTAL")]
	public class GraphQLSubscriptionResult : IDisposable {

		/// <summary>
		/// Invoked when a GraphQLResponse is received
		/// </summary>
		public event Action<GraphQLResponse> OnGraphQLResponse;

		private readonly Uri uri;
		private readonly CancellationToken cancellationToken;
		private readonly ClientWebSocket clientWebSocket = new ClientWebSocket();

		/// <summary>
		/// Create a new instance of <see cref="GraphQLSubscriptionResult"/>
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="cancellationToken"></param>
		public GraphQLSubscriptionResult(Uri uri, CancellationToken cancellationToken = default) {
			this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
			this.cancellationToken = cancellationToken;
		}

		/// <summary>
		/// Connects the <see cref="ClientWebSocket"/>
		/// </summary>
		/// <returns></returns>
		public async Task ConnectAsync() =>
			await this.clientWebSocket.ConnectAsync(this.uri, this.cancellationToken).ConfigureAwait(false);

		/// <summary>
		/// Start Lisenting
		/// </summary>
		public async void StartAsync() {
			await this.ReceiveAsync();
		}

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose() => this.clientWebSocket.Dispose();


		private async Task ReceiveAsync() {
			var buffer = new byte[1024];
			var bufferSegment = new ArraySegment<byte>(buffer);

			var webSocketReceiveResult = await this.clientWebSocket.ReceiveAsync(bufferSegment, this.cancellationToken);
			if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close) {
				await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this.cancellationToken);
				return;
			}

			var count = webSocketReceiveResult.Count;
			while (!webSocketReceiveResult.EndOfMessage) {
				if (count >= buffer.Length) {
					await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "That's too long", this.cancellationToken);
					return;
				}

				bufferSegment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
				webSocketReceiveResult = await this.clientWebSocket.ReceiveAsync(bufferSegment, this.cancellationToken);
				count += webSocketReceiveResult.Count;
			}

			var message = Encoding.UTF8.GetString(buffer, 0, count);
			var graphQLResponse = JsonConvert.DeserializeObject<GraphQLResponse>(message);
			this.OnGraphQLResponse?.Invoke(graphQLResponse);
		}

	}

}
