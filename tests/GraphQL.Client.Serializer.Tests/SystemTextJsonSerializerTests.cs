using GraphQL.Client.Serializer.SystemTextJson;

namespace GraphQL.Client.Serializer.Tests {
	public class SystemTextJsonSerializerTests: BaseSerializerTest {
		public SystemTextJsonSerializerTests() : base(new SystemTextJsonSerializer())
		{
		}
	}
}
