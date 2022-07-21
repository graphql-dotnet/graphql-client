using GraphQL.Client.Http;

namespace GraphQL.Client.Tests.Common.Helpers;

public static class MiscellaneousExtensions
{
    public static string RemoveWhitespace(this string input) =>
        new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());

    public static CallbackMonitor<GraphQLHttpClient> ConfigureMonitorForOnWebsocketConnected(
        this GraphQLHttpClient client)
    {
        var tester = new CallbackMonitor<GraphQLHttpClient>();
        client.Options.OnWebsocketConnected = c =>
        {
            tester.Invoke(c);
            return Task.CompletedTask;
        };
        return tester;
    }
}
