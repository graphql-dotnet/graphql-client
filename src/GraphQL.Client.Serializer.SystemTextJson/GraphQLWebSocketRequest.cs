using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class GraphQLWebSocketRequest: Abstractions.Websocket.GraphQLWebSocketRequest {

		[JsonPropertyName(IdKey)]
		public override string Id { get; set; }
		[JsonPropertyName(TypeKey)]
		public override string Type { get; set; }
		[JsonPropertyName(PayloadKey)]
		public override GraphQL.GraphQLRequest Payload { get; set; }

		public GraphQLWebSocketRequest()
		{
		}

		public GraphQLWebSocketRequest(Abstractions.Websocket.GraphQLWebSocketRequest other) {
			Id = other.Id;
			Type = other.Type;
			Payload = other.Payload != null ? new GraphQLRequest(other.Payload) : null; // create serializer-specific type;
		}
	}
}
