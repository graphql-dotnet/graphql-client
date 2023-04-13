using FluentAssertions;
using GraphQL.Client.Http.Websocket;
using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class SystemTextJsonAutoNegotiate : Base, IClassFixture<SystemTextJsonAutoNegotiateServerTestFixture>
{
    public SystemTextJsonAutoNegotiate(ITestOutputHelper output, SystemTextJsonAutoNegotiateServerTestFixture fixture) : base(output, fixture)
    {
    }

    [Fact]
    public async Task WebSocketProtocolIsAutoNegotiated()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);
        ChatClient.WebSocketSubProtocol.Should().Be(WebSocketProtocols.GRAPHQL_TRANSPORT_WS);
    }
}
