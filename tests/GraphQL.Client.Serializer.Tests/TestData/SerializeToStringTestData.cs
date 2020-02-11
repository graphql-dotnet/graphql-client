using System.Collections;
using System.Collections.Generic;
using GraphQL.Client.Tests.Common.Chat;

namespace GraphQL.Client.Serializer.Tests.TestData {
	public class SerializeToStringTestData : IEnumerable<object[]> {
		public IEnumerator<object[]> GetEnumerator() {
			yield return new object[] {
				"{\"query\":\"simple query string\",\"operationName\":null,\"variables\":null}",
				new GraphQLRequest("simple query string")
			};
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
