using System.Collections;
using System.Collections.Generic;
using GraphQL.Client.Tests.Common.Chat;

namespace GraphQL.Client.Serializer.Tests.TestData {
	public class SerializeToStringTestData : IEnumerable<object[]> {
		public IEnumerator<object[]> GetEnumerator() {
			yield return new object[] {
				"{\"query\":\"simplequerystring\",\"variables\":null,\"operationName\":null}",
				new GraphQLRequest("simple query string")
			};
			yield return new object[] {
				"{\"query\":\"simplequerystring\",\"variables\":{\"camelCaseProperty\":\"camelCase\",\"PascalCaseProperty\":\"PascalCase\"},\"operationName\":null}",
				new GraphQLRequest("simple query string", new { camelCaseProperty = "camelCase", PascalCaseProperty = "PascalCase"})
			};
			yield return new object[] {
				"{\"query\":\"simplequerystring\",\"variables\":null,\"operationName\":null,\"authentication\":\"an-authentication-token\"}",
				new GraphQLRequest("simple query string"){{"authentication", "an-authentication-token"}}
			};
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
