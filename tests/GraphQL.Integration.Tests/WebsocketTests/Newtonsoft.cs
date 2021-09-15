using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests
{
    public class Newtonsoft : BaseWithTimeout, IClassFixture<NewtonsoftIntegrationServerTestFixture>
    {
        public Newtonsoft(ITestOutputHelper output, NewtonsoftIntegrationServerTestFixture fixture) : base(output, fixture)
        {
        }
    }
}
