using System.Net.Http.Headers;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests;

public class UserAgentHeaderTests : IAsyncLifetime, IClassFixture<SystemTextJsonAutoNegotiateServerTestFixture>
{
    private readonly IntegrationServerTestFixture Fixture;
    private GraphQLHttpClient? ChatClient;

    public UserAgentHeaderTests(SystemTextJsonAutoNegotiateServerTestFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync() => await Fixture.CreateServer().ConfigureAwait(false);

    public Task DisposeAsync()
    {
        ChatClient?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async void Can_set_custom_user_agent()
    {
        const string userAgent = "CustomUserAgent";
        ChatClient = Fixture.GetChatClient(options => options.DefaultUserAgentRequestHeader = ProductInfoHeaderValue.Parse(userAgent));

        var graphQLRequest = new GraphQLRequest("query clientUserAgent { clientUserAgent }");
        var response = await ChatClient.SendQueryAsync(graphQLRequest, () => new { clientUserAgent = string.Empty }).ConfigureAwait(false);

        response.Errors.Should().BeNull();
        response.Data.clientUserAgent.Should().Be(userAgent);
    }

    [Fact]
    public async void Default_user_agent_is_set_as_expected()
    {
        string? expectedUserAgent = new ProductInfoHeaderValue(
            typeof(GraphQLHttpClient).Assembly.GetName().Name,
            typeof(GraphQLHttpClient).Assembly.GetName().Version.ToString()).ToString();

        ChatClient = Fixture.GetChatClient();

        var graphQLRequest = new GraphQLRequest("query clientUserAgent { clientUserAgent }");
        var response = await ChatClient.SendQueryAsync(graphQLRequest, () => new { clientUserAgent = string.Empty }).ConfigureAwait(false);

        response.Errors.Should().BeNull();
        response.Data.clientUserAgent.Should().Be(expectedUserAgent);
    }

    [Fact]
    public async void No_Default_user_agent_if_set_to_null()
    {
        ChatClient = Fixture.GetChatClient(options => options.DefaultUserAgentRequestHeader = null);

        var graphQLRequest = new GraphQLRequest("query clientUserAgent { clientUserAgent }");
        var response = await ChatClient.SendQueryAsync(graphQLRequest, () => new { clientUserAgent = string.Empty }).ConfigureAwait(false);

        response.Errors.Should().HaveCount(1);
        response.Errors[0].Message.Should().Be("user agent header not set");
    }
}
