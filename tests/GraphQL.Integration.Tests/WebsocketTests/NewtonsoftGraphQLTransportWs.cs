using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class NewtonsoftGraphQLTransportWs : Base, IClassFixture<NewtonsoftGraphQLTransportWsServerTestFixture>
{
    public NewtonsoftGraphQLTransportWs(ITestOutputHelper output, NewtonsoftGraphQLTransportWsServerTestFixture fixture) : base(output, fixture)
    {
    }
}
