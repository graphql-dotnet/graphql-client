using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class NewtonsoftGraphQLWs : Base, IClassFixture<NewtonsoftGraphQLWsServerTestFixture>
{
    public NewtonsoftGraphQLWs(ITestOutputHelper output, NewtonsoftGraphQLWsServerTestFixture fixture) : base(output, fixture)
    {
    }
}
