using FluentAssertions;
using FluentAssertions.Reactive;
using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class SystemTextJsonGraphQLTransportWs : Base, IClassFixture<SystemTextJsonGraphQLTransportWsServerTestFixture>
{
    public SystemTextJsonGraphQLTransportWs(ITestOutputHelper output, SystemTextJsonGraphQLTransportWsServerTestFixture fixture) : base(output, fixture)
    {
    }

    [Fact]
    public async void Sending_a_pong_message_should_not_throw()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);

        await ChatClient.Awaiting(client => client.SendPongAsync(null)).Should().NotThrowAsync().ConfigureAwait(false);
        await ChatClient.Awaiting(client => client.SendPongAsync("some payload")).Should().NotThrowAsync().ConfigureAwait(false);
    }

    [Fact]
    public async void Sending_a_ping_message_should_result_in_a_pong_message_from_the_server()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);

        using var pongObserver = ChatClient.PongStream.Observe();

        await ChatClient.Awaiting(client => client.SendPingAsync(null)).Should().NotThrowAsync().ConfigureAwait(false);

        await pongObserver.Should().PushAsync(1, TimeSpan.FromSeconds(1), "because the server was pinged by the client").ConfigureAwait(false);
    }
}
