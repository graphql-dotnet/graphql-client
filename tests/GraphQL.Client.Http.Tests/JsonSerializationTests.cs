using System.Globalization;
using Xunit;
using System.Text.Json;

namespace GraphQL.Client.Http.Tests {
	public class JsonSerializationTests {

		[Fact]
		public void WebSocketResponseDeserialization() {
			var testObject = new ExtendedTestObject { Id = "test", OtherData = "this is some other stuff" };
			var json = JsonSerializer.Serialize(testObject);
			var deserialized = JsonSerializer.Deserialize<TestObject>(json);

		}

		public class TestObject {
			public string Id { get; set; }

		}

		public class ExtendedTestObject : TestObject {
			public string OtherData { get; set; }
		}
	}
}
