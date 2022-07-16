using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Helpers;

namespace GraphQL.Integration.Tests.Helpers
{
    public abstract class IntegrationServerTestFixture
    {
        public int Port { get; private set; }

        public IWebHost Server { get; private set; }

        public abstract IGraphQLWebsocketJsonSerializer Serializer { get; }

        public IntegrationServerTestFixture()
        {
            Port = NetworkHelpers.GetFreeTcpPortNumber();
        }

        public async Task CreateServer()
        {
            if (Server != null)
                return;
            Server = await WebHostHelpers.CreateServer(Port);
        }

        public async Task ShutdownServer()
        {
            if (Server == null)
                return;

            await Server.StopAsync();
            Server.Dispose();
            Server = null;
        }

        public GraphQLHttpClient GetStarWarsClient(bool requestsViaWebsocket = false)
            => GetGraphQLClient(Common.STAR_WARS_ENDPOINT, requestsViaWebsocket);

        public GraphQLHttpClient GetChatClient(bool requestsViaWebsocket = false)
            => GetGraphQLClient(Common.CHAT_ENDPOINT, requestsViaWebsocket);

        private GraphQLHttpClient GetGraphQLClient(string endpoint, bool requestsViaWebsocket = false)
        {
            if (Serializer == null)
                throw new InvalidOperationException("JSON serializer not configured");
            return WebHostHelpers.GetGraphQLClient(Port, endpoint, requestsViaWebsocket, Serializer);
        }
    }

    public class NewtonsoftIntegrationServerTestFixture : IntegrationServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    }

    public class SystemTextJsonIntegrationServerTestFixture : IntegrationServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    }
}
