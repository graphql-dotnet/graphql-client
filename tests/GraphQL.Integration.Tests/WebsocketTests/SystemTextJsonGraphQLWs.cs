using FluentAssertions;
using GraphQL.Integration.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class SystemTextJsonGraphQLWs : Base, IClassFixture<SystemTextJsonGraphQLWsServerTestFixture>
{
    public SystemTextJsonGraphQLWs(ITestOutputHelper output, SystemTextJsonGraphQLWsServerTestFixture fixture) : base(output, fixture)
    {
    }

    [Fact]
    public async void Sending_a_ping_message_should_throw_not_supported_exception()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);

        await ChatClient.Awaiting(client => client.SendPingAsync(null))
            .Should().ThrowAsync<NotSupportedException>().ConfigureAwait(false);
    }

    [Fact]
    public async void Sending_a_pong_message_should_throw_not_supported_exception()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);

        await ChatClient.Awaiting(client => client.SendPongAsync(null))
            .Should().ThrowAsync<NotSupportedException>().ConfigureAwait(false);
    }

    [Fact]
    public async void Subscribing_to_the_pong_stream_should_throw_not_supported_exception()
    {
        await ChatClient.InitializeWebsocketConnection().ConfigureAwait(false);

        ChatClient.Invoking(client => client.PongStream.Subscribe())
            .Should().Throw<NotSupportedException>();
    }
}
