using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests
{
    public class TestServerNewtonsoft : Base, IClassFixture<TestServerTestNewtonsoftFixture>
    {
        public TestServerNewtonsoft(ITestOutputHelper output, TestServerTestNewtonsoftFixture fixture) : base(output, fixture)
        {
            fixture.Output = output;
        }
    }
}
