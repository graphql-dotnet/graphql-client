using System.Runtime.Serialization;

namespace GraphQL.Client.Http.Websocket {
	public class WebsocketResponseWrapper : GraphQLWebSocketResponse {

		[IgnoreDataMember]
		public byte[] MessageBytes { get; set; }
	}
}
