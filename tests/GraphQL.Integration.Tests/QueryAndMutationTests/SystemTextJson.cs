using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests {
	public class SystemTextJson: Base, IClassFixture<SystemTextJsonIntegrationServerTestFixture> {
		public SystemTextJson(SystemTextJsonIntegrationServerTestFixture fixture) : base(fixture)
		{
		}
	}
}
