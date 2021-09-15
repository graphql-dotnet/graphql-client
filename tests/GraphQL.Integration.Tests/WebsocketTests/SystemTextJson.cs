using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests
{
    public class SystemTextJson : BaseWithTimeout, IClassFixture<SystemTextJsonIntegrationServerTestFixture>
    {
        public SystemTextJson(ITestOutputHelper output, SystemTextJsonIntegrationServerTestFixture fixture) : base(output, fixture)
        {
        }
    }
}
