using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Newtonsoft {
	public class GraphQLWebSocketRequest: Abstractions.Websocket.GraphQLWebSocketRequest {

		[JsonProperty(IdKey)]
		public override string Id { get; set; }
		[JsonProperty(TypeKey)]
		public override string Type { get; set; }
		[JsonProperty(PayloadKey)]
		public override GraphQL.GraphQLRequest Payload { get; set; }

		public GraphQLWebSocketRequest()
		{
		}

		public GraphQLWebSocketRequest(Abstractions.Websocket.GraphQLWebSocketRequest other) {
			Id = other.Id;
			Type = other.Type;
			Payload = new Newtonsoft.GraphQLRequest(other.Payload);
		}
	}
}
