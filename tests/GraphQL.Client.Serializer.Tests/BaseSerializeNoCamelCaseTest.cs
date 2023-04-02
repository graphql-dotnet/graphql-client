using System.Text;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.LocalExecution;
using GraphQL.Client.Serializer.Tests.TestData;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Helpers;
using Xunit;

namespace GraphQL.Client.Serializer.Tests;

public abstract class BaseSerializeNoCamelCaseTest
{
    public IGraphQLWebsocketJsonSerializer ClientSerializer { get; }

    public IGraphQLTextSerializer ServerSerializer { get; }

    public IGraphQLClient ChatClient { get; }

    public IGraphQLClient StarWarsClient { get; }

    protected BaseSerializeNoCamelCaseTest(IGraphQLWebsocketJsonSerializer clientSerializer, IGraphQLTextSerializer serverSerializer)
    {
        ClientSerializer = clientSerializer;
        ServerSerializer = serverSerializer;
        ChatClient = GraphQLLocalExecutionClient.New(Common.GetChatSchema(), clientSerializer, serverSerializer);
        StarWarsClient = GraphQLLocalExecutionClient.New(Common.GetStarWarsSchema(), clientSerializer, serverSerializer);
    }

    [Theory]
    [ClassData(typeof(SerializeToStringTestData))]
    public void SerializeToStringTest(string expectedJson, GraphQLRequest request)
    {
        var json = ClientSerializer.SerializeToString(request).RemoveWhitespace();
        json.Should().Be(expectedJson.RemoveWhitespace());
    }

    [Theory]
    [ClassData(typeof(SerializeToBytesTestData))]
    public void SerializeToBytesTest(string expectedJson, GraphQLWebSocketRequest request)
    {
        var json = Encoding.UTF8.GetString(ClientSerializer.SerializeToBytes(request)).RemoveWhitespace();
        json.Should().Be(expectedJson.RemoveWhitespace());
    }

    [Fact]
    public async void WorksWithoutCamelCaseNamingStrategy()
    {
        const string message = "some random testing message";
        var graphQLRequest = new GraphQLRequest(
            @"mutation($input: MessageInputType){
				  addMessage(message: $input){
				    content
				  }
				}",
            new
            {
                input = new
                {
                    fromId = "2",
                    content = message,
                    sentAt = DateTime.Now
                }
            });
        var response = await ChatClient.SendMutationAsync(graphQLRequest, () => new { addMessage = new { content = "" } });

        Assert.Equal(message, response.Data.addMessage.content);
    }
}
