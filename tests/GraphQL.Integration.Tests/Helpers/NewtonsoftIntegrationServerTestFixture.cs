using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Integration.Tests.Helpers
{
    public class NewtonsoftIntegrationServerTestFixture : IntegrationServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    }
}