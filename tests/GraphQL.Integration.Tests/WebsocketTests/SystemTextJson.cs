using GraphQL.Client.Serializer.SystemTextJson;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests {
	public class SystemTextJson: Base {
		public SystemTextJson(ITestOutputHelper output) : base(output, new SystemTextJsonSerializer())
		{
		}
	}
}
