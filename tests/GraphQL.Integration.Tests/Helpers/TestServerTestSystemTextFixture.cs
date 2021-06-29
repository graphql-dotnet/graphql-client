using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.SystemTextJson;

namespace GraphQL.Integration.Tests.Helpers
{
    public class TestServerTestSystemTextFixture : TestServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    }
}