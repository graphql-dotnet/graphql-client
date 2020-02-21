using System.Text.Json;
using Dahomey.Json;
using GraphQL.Client.Serializer.SystemTextJson;

namespace GraphQL.Client.Serializer.Tests {
	public class SystemTextJsonSerializerTests: BaseSerializerTest {
		public SystemTextJsonSerializerTests() : base(new SystemTextJsonSerializer()) { }
	}

	public class SystemTextJsonSerializeNoCamelCaseTest : BaseSerializeNoCamelCaseTest {
		public SystemTextJsonSerializeNoCamelCaseTest()
			: base(new SystemTextJsonSerializer(new JsonSerializerOptions().SetupExtensions())) { }
	}
}
