using System.Collections;
using System.Collections.Generic;
using GraphQL.Client.Tests.Common.Chat;

namespace GraphQL.Client.Serializer.Tests.TestData {
	public class SerializeToStringTestData : IEnumerable<object[]> {
		public IEnumerator<object[]> GetEnumerator() {
			yield return new object[] {
				"{\"query\":\"simplequerystring\",\"operationName\":null,\"variables\":null}",
				new GraphQLRequest("simple query string")
			};
			yield return new object[] {
				"{\"query\":\"simplequerystring\",\"operationName\":null,\"variables\":{\"camelCaseProperty\":\"test\",\"pascalCaseProperty\":\"test\"}}",
				new GraphQLRequest("simple query string", new { camelCaseProperty = "test", PascalCaseProperty = "test"})
			};
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
