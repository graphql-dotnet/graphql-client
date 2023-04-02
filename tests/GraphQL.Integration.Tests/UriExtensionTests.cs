using FluentAssertions;
using GraphQL.Client.Http;
using Xunit;

namespace GraphQL.Integration.Tests;

public class UriExtensionTests
{
    [Theory]
    [InlineData("http://thats-not-a-websocket-url.net", false)]
    [InlineData("https://thats-not-a-websocket-url.net", false)]
    [InlineData("ftp://thats-not-a-websocket-url.net", false)]
    [InlineData("ws://that-is-a-websocket-url.net", true)]
    [InlineData("wss://that-is-a-websocket-url.net", true)]
    [InlineData("WS://that-is-a-websocket-url.net", true)]
    [InlineData("WSS://that-is-a-websocket-url.net", true)]
    public void HasWebSocketSchemaTest(string url, bool result)
    {
        new Uri(url).HasWebSocketScheme().Should().Be(result);
    }

    [Theory]
    [InlineData("http://this-url-can-be-converted.net", true, "ws://this-url-can-be-converted.net")]
    [InlineData("https://this-url-can-be-converted.net", true, "wss://this-url-can-be-converted.net")]
    [InlineData("HTTP://this-url-can-be-converted.net", true, "ws://this-url-can-be-converted.net")]
    [InlineData("HTTPS://this-url-can-be-converted.net", true, "wss://this-url-can-be-converted.net")]
    [InlineData("ws://this-url-can-be-converted.net", true, "ws://this-url-can-be-converted.net")]
    [InlineData("wss://this-url-can-be-converted.net", true, "wss://this-url-can-be-converted.net")]
    [InlineData("https://this-url-can-be-converted.net/and/all/elements/?are#preserved", true, "wss://this-url-can-be-converted.net/and/all/elements/?are#preserved")]
    [InlineData("ftp://this-url-cannot-be-converted.net", false, null)]
    // AppSync example
    [InlineData("wss://example1234567890000.appsync-realtime-api.us-west-2.amazonaws.com/graphql?header=123456789ABCDEF&payload=e30=", true, "wss://example1234567890000.appsync-realtime-api.us-west-2.amazonaws.com/graphql?header=123456789ABCDEF&payload=e30=")]
    public void GetWebSocketUriTest(string input, bool canConvert, string result)
    {
        var inputUri = new Uri(input);
        if (canConvert)
        {
            inputUri.GetWebSocketUri().Should().BeEquivalentTo(new Uri(result));
        }
        else
        {
            inputUri.Invoking(uri => uri.GetWebSocketUri()).Should().Throw<NotSupportedException>();
        }
    }
}
