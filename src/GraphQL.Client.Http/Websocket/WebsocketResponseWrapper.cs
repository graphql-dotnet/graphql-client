using System.Text.Json.Serialization;

namespace GraphQL.Client.Http.Websocket {
	public class WebsocketResponseWrapper : GraphQLWebSocketResponse {
		
		[JsonIgnore]
		public byte[] MessageBytes { get; set; }
	}
}
