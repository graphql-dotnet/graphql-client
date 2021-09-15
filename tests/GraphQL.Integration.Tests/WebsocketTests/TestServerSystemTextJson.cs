using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests
{
    public class TestServerSystemTextJson : Base, IClassFixture<TestServerTestSystemTextFixture>
    {
        public TestServerSystemTextJson(ITestOutputHelper output, TestServerTestSystemTextFixture fixture) : base(output, fixture)
        {
            fixture.Output = output;
        }
    }
}
