using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.LocalExecution;
using GraphQL.Client.Serializer.Tests.TestData;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.Helpers;
using GraphQL.Client.Tests.Common.StarWars;
using Xunit;

namespace GraphQL.Client.Serializer.Tests
{
    public abstract class BaseSerializerTest
    {
	    public IGraphQLWebsocketJsonSerializer Serializer { get; }
	    public IGraphQLClient ChatClient { get; }
	    public IGraphQLClient StarWarsClient { get; }

		protected BaseSerializerTest(IGraphQLWebsocketJsonSerializer serializer) {
			Serializer = serializer;
			ChatClient = GraphQLLocalExecutionClient.New(Common.GetChatSchema(), serializer);
			StarWarsClient = GraphQLLocalExecutionClient.New(Common.GetStarWarsSchema(), serializer);
		}

		[Theory]
		[ClassData(typeof(SerializeToStringTestData))]
		public void SerializeToStringTest(string expectedJson, GraphQLRequest request) {
			var json = Serializer.SerializeToString(request).RemoveWhitespace();
			json.Should().BeEquivalentTo(expectedJson.RemoveWhitespace());
		}

		[Theory]
		[ClassData(typeof(SerializeToBytesTestData))]
		public void SerializeToBytesTest(string expectedJson, GraphQLWebSocketRequest request) {
			var json = Encoding.UTF8.GetString(Serializer.SerializeToBytes(request)).RemoveWhitespace();
			json.Should().BeEquivalentTo(expectedJson.RemoveWhitespace());
		}

		[Theory]
		[ClassData(typeof(DeserializeResponseTestData))]
		public async void DeserializeFromUtf8StreamTest(string json, GraphQLResponse<object> expectedResponse) {
			var jsonBytes = Encoding.UTF8.GetBytes(json);
			await using var ms = new MemoryStream(jsonBytes);
			var response = await Serializer.DeserializeFromUtf8StreamAsync<GraphQLResponse<object>>(ms, CancellationToken.None);

			response.Data.Should().BeEquivalentTo(expectedResponse.Data);
			response.Errors.Should().Equal(expectedResponse.Errors);

			if (expectedResponse.Extensions == null)
				response.Extensions.Should().BeNull();
			else {
				foreach (var element in expectedResponse.Extensions) {
					response.Extensions.Should().ContainKey(element.Key);
					response.Extensions[element.Key].Should().BeEquivalentTo(element.Value);
				}
			}
		}

		[Fact]
		public async void CanDeserializeExtensions() {

			var response = await ChatClient.SendQueryAsync(new GraphQLRequest("query { extensionsTest }"),
					() => new { extensionsTest = "" })
				;

			response.Errors.Should().NotBeNull();
			response.Errors.Should().ContainSingle();
			response.Errors[0].Extensions.Should().NotBeNull();
			response.Errors[0].Extensions.Should().ContainKey("data");

			response.Errors[0].Extensions["data"].Should().BeEquivalentTo(ChatQuery.TestExtensions);
		}


		[Theory]
		[ClassData(typeof(StarWarsHumans))]
		public async void CanDoSerializationWithAnonymousTypes(int id, string name) {
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
		public async void CanDoSerializationWithPredefinedTypes() {
				const string message = "some random testing message";
				var response = await ChatClient.AddMessageAsync(message);

				Assert.Equal(message, response.Data.AddMessage.Content);
		}
	}
}
