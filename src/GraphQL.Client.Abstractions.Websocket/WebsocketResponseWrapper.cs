using System.Runtime.Serialization;

namespace GraphQL.Client.Abstractions.Websocket {
	public class WebsocketResponseWrapper : GraphQLWebSocketResponse {

		[IgnoreDataMember]
		public byte[] MessageBytes { get; set; }
	}
}
