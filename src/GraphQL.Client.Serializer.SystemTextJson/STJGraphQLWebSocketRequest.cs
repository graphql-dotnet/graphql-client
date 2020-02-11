using System.Text.Json.Serialization;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class STJGraphQLWebSocketRequest: GraphQLWebSocketRequest {

		[JsonPropertyName(IdKey)]
		public override string Id { get; set; }
		[JsonPropertyName(TypeKey)]
		public override string Type { get; set; }
		[JsonPropertyName(PayloadKey)]
		public override GraphQLRequest Payload { get; set; }

		public STJGraphQLWebSocketRequest()
		{
		}

		public STJGraphQLWebSocketRequest(GraphQLWebSocketRequest other) {
			Id = other.Id;
			Type = other.Type;
			Payload = new STJGraphQLRequest(other.Payload);
		}
	}
}
