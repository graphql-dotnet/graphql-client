using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Integration.Tests.Helpers
{
    public class TestServerTestNewtonsoftFixture : TestServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    }
}