using System.Collections;
using System.Collections.Generic;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Tests.Common.Chat;

namespace GraphQL.Client.Serializer.Tests.TestData {
	public class SerializeToBytesTestData : IEnumerable<object[]> {
		public IEnumerator<object[]> GetEnumerator() {
			yield return new object[] {
				"{\"id\":\"1234567\",\"type\":\"start\",\"payload\":{\"query\":\"simplequerystring\",\"variables\":null,\"operationName\":null}}",
				new GraphQLWebSocketRequest {
					Id = "1234567",
					Type = GraphQLWebSocketMessageType.GQL_START,
					Payload = new GraphQLRequest("simplequerystring")
				}
			};
			yield return new object[] {
				"{\"id\":\"34476567\",\"type\":\"start\",\"payload\":{\"query\":\"simplequerystring\",\"variables\":{\"camelCaseProperty\":\"camelCase\",\"PascalCaseProperty\":\"PascalCase\"},\"operationName\":null}}",
				new GraphQLWebSocketRequest {
					Id = "34476567",
					Type = GraphQLWebSocketMessageType.GQL_START,
					Payload = new GraphQLRequest("simple query string", new { camelCaseProperty = "camelCase", PascalCaseProperty = "PascalCase"})
				}
				
			};
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
