using System.Reflection;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.LocalExecution;
using GraphQL.Client.Serializer.Tests.TestData;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Client.Tests.Common.StarWars.TestData;
using Xunit;

namespace GraphQL.Client.Serializer.Tests;

public abstract class BaseSerializerTest
{
    public IGraphQLWebsocketJsonSerializer ClientSerializer { get; }

    public IGraphQLTextSerializer ServerSerializer { get; }

    public IGraphQLClient ChatClient { get; }

    public IGraphQLClient StarWarsClient { get; }

    protected BaseSerializerTest(IGraphQLWebsocketJsonSerializer clientSerializer, IGraphQLTextSerializer serverSerializer)
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
        json.Should().BeEquivalentTo(expectedJson.RemoveWhitespace());
    }

    [Theory]
    [ClassData(typeof(SerializeToBytesTestData))]
    public void SerializeToBytesTest(string expectedJson, GraphQLWebSocketRequest request)
    {
        var json = Encoding.UTF8.GetString(ClientSerializer.SerializeToBytes(request)).RemoveWhitespace();
        json.Should().BeEquivalentTo(expectedJson.RemoveWhitespace());
    }

    [Theory]
    [ClassData(typeof(DeserializeResponseTestData))]
    public async void DeserializeFromUtf8StreamTest(string json, IGraphQLResponse expectedResponse)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        await using var ms = new MemoryStream(jsonBytes);
        var response = await DeserializeToUnknownType(expectedResponse.Data?.GetType() ?? typeof(object), ms);

        //var response = await Serializer.DeserializeFromUtf8StreamAsync<object>(ms, CancellationToken.None);

        response.Data.Should().BeEquivalentTo(expectedResponse.Data, options => options.WithAutoConversion());

        if (expectedResponse.Errors is null)
            response.Errors.Should().BeNull();
        else
        {
            using (new AssertionScope())
            {
                response.Errors.Should().NotBeNull();
                response.Errors.Should().HaveSameCount(expectedResponse.Errors);
                for (int i = 0; i < expectedResponse.Errors.Length; i++)
                {
                    response.Errors[i].Message.Should().BeEquivalentTo(expectedResponse.Errors[i].Message);
                    response.Errors[i].Locations.Should().BeEquivalentTo(expectedResponse.Errors[i].Locations?.ToList());
                    response.Errors[i].Path.Should().BeEquivalentTo(expectedResponse.Errors[i].Path);
                    response.Errors[i].Extensions.Should().BeEquivalentTo(expectedResponse.Errors[i].Extensions);
                }
            }
        }

        if (expectedResponse.Extensions == null)
            response.Extensions.Should().BeNull();
        else
        {
            foreach (var element in expectedResponse.Extensions)
            {
                response.Extensions.Should().ContainKey(element.Key);
                response.Extensions[element.Key].Should().BeEquivalentTo(element.Value);
            }
        }
    }

    public async Task<IGraphQLResponse> DeserializeToUnknownType(Type dataType, Stream stream)
    {
        MethodInfo mi = ClientSerializer.GetType().GetMethod("DeserializeFromUtf8StreamAsync", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo mi2 = mi.MakeGenericMethod(dataType);
        var task = (Task)mi2.Invoke(ClientSerializer, new object[] { stream, CancellationToken.None });
        await task;
        var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
        var result = resultProperty.GetValue(task);
        return (IGraphQLResponse)result;
    }

    [Fact]
    public async void CanDeserializeExtensions()
    {
        var response = await ChatClient.SendQueryAsync(
            new GraphQLRequest("query { extensionsTest }"),
            () => new { extensionsTest = "" });

        response.Errors.Should().NotBeNull();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Extensions.Should().NotBeNull();
        response.Errors[0].Extensions.Should().ContainKey("data");

        response.Errors[0].Extensions["data"].Should().BeEquivalentTo(ChatQuery.TestExtensions);
    }

    [Theory]
    [ClassData(typeof(StarWarsHumans))]
    public async void CanDoSerializationWithAnonymousTypes(int id, string name)
    {
        var graphQLRequest = new GraphQLRequest(@"
				query Human($id: String!){
					human(id: $id) {
						name
					}
				}

				query Droid($id: String!) {
				  droid(id: $id) {
				    name
				  }
				}",
            new { id = id.ToString() },
            "Human");

        var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });

        Assert.Null(response.Errors);
        Assert.Equal(name, response.Data.Human.Name);
    }

    [Fact]
    public async void CanDoSerializationWithPredefinedTypes()
    {
        const string message = "some random testing message";
        var response = await ChatClient.AddMessageAsync(message);

        Assert.Equal(message, response.Data.AddMessage.Content);
    }

    public class WithNullable
    {
        public int? NullableInt { get; set; }
    }

    [Fact]
    public void CanSerializeNullableInt()
    {
        Action action = () => ClientSerializer.SerializeToString(new GraphQLRequest
        {
            Query = "{}",
            Variables = new WithNullable
            {
                NullableInt = 2
            }
        });

        action.Should().NotThrow();
    }

    public class WithNullableStruct
    {
        public DateTime? NullableStruct { get; set; }
    }

    [Fact]
    public void CanSerializeNullableStruct()
    {
        Action action = () => ClientSerializer.SerializeToString(new GraphQLRequest
        {
            Query = "{}",
            Variables = new WithNullableStruct
            {
                NullableStruct = DateTime.Now
            }
        });

        action.Should().NotThrow();
    }
}
