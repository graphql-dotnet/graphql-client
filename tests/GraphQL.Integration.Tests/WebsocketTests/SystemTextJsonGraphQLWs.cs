using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class SystemTextJsonGraphQLWs : Base, IClassFixture<SystemTextJsonGraphQLWsServerTestFixture>
{
    public SystemTextJsonGraphQLWs(ITestOutputHelper output, SystemTextJsonGraphQLWsServerTestFixture fixture) : base(output, fixture)
    {
    }
}
