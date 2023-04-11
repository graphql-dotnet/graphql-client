using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class SystemTextJsonGraphQLTransportWs : Base, IClassFixture<SystemTextJsonGraphQLTransportWsServerTestFixture>
{
    public SystemTextJsonGraphQLTransportWs(ITestOutputHelper output, SystemTextJsonGraphQLTransportWsServerTestFixture fixture) : base(output, fixture)
    {
    }
}
