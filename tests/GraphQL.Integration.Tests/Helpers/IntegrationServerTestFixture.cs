using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Http.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Helpers;

namespace GraphQL.Integration.Tests.Helpers;

public abstract class IntegrationServerTestFixture
{
    public int Port { get; private set; }

    public IWebHost? Server { get; private set; }

    public abstract IGraphQLWebsocketJsonSerializer Serializer { get; }

    public abstract string? WebsocketProtocol { get; }

    public IntegrationServerTestFixture()
    {
        Port = NetworkHelpers.GetFreeTcpPortNumber();
    }

    public async Task CreateServer()
    {
        if (Server != null)
            return;
        Server = await WebHostHelpers.CreateServer(Port).ConfigureAwait(false);
    }

    public async Task ShutdownServer()
    {
        if (Server == null)
            return;

        await Server.StopAsync();
        Server.Dispose();
        Server = null;
    }

    public GraphQLHttpClient GetStarWarsClient(Action<GraphQLHttpClientOptions>? configure = null)
        => GetGraphQLClient(Common.STAR_WARS_ENDPOINT, configure);

    public GraphQLHttpClient GetChatClient(Action<GraphQLHttpClientOptions>? configure = null)
        => GetGraphQLClient(Common.CHAT_ENDPOINT, configure);

    private GraphQLHttpClient GetGraphQLClient(string endpoint, Action<GraphQLHttpClientOptions>? configure) =>
        Serializer == null
            ? throw new InvalidOperationException("JSON serializer not configured")
            : WebHostHelpers.GetGraphQLClient(Port, endpoint, Serializer, options =>
            {
                configure?.Invoke(options);
                options.WebSocketProtocol = WebsocketProtocol;
            });
}

public class NewtonsoftGraphQLWsServerTestFixture : IntegrationServerTestFixture
{
    public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    public override string? WebsocketProtocol => WebSocketProtocols.GRAPHQL_WS;
}

public class SystemTextJsonGraphQLWsServerTestFixture : IntegrationServerTestFixture
{
    public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    public override string? WebsocketProtocol => WebSocketProtocols.GRAPHQL_WS;
}

public class NewtonsoftGraphQLTransportWsServerTestFixture : IntegrationServerTestFixture
{
    public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    public override string? WebsocketProtocol => WebSocketProtocols.GRAPHQL_TRANSPORT_WS;
}

public class SystemTextJsonGraphQLTransportWsServerTestFixture : IntegrationServerTestFixture
{
    public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    public override string? WebsocketProtocol => WebSocketProtocols.GRAPHQL_TRANSPORT_WS;
}
public class SystemTextJsonAutoNegotiateServerTestFixture : IntegrationServerTestFixture
{
    public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    public override string? WebsocketProtocol => WebSocketProtocols.AUTO_NEGOTIATE;
}
