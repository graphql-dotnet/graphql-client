using System.Collections.Generic;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.LocalExecution;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Chat;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.StarWars;
using Xunit;

namespace GraphQL.Client.Serializer.Tests
{
    public abstract class BaseSerializerTest
    {
	    public IGraphQLClient ChatClient { get; }
	    public IGraphQLClient StarWarsClient { get; }

		protected BaseSerializerTest(IGraphQLWebsocketJsonSerializer serializer) {
			ChatClient = GraphQLLocalExecutionClient.New(Common.GetChatSchema(), serializer);
			StarWarsClient = GraphQLLocalExecutionClient.New(Common.GetStarWarsSchema(), serializer);
		}

		[Fact]
		public async void CanDeserializeExtensions() {

			var response = await ChatClient.SendQueryAsync(new GraphQLRequest("query { extensionsTest }"),
					() => new { extensionsTest = "" })
				.ConfigureAwait(false);

			response.Errors.Should().NotBeNull();
			response.Errors.Should().ContainSingle();
			response.Errors[0].Extensions.Should().NotBeNull();
			response.Errors[0].Extensions.Should().ContainKey("data");

			AssertGraphQlErrorDataExtensions(response.Errors[0].Extensions["data"], ChatQuery.TestExtensions);
		}

		/// <summary>
		/// serializer-specific assertion of the <see cref="GraphQLError.Extensions"/> field
		/// </summary>
		/// <param name="dataField">the field with key "data" from <see cref="GraphQLError.Extensions"/></param>
		/// <param name="expectedContent"><see cref="ChatQuery.TestExtensions"/></param>
		protected abstract void AssertGraphQlErrorDataExtensions(object dataField, IDictionary<string, object> expectedContent);

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

			var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } })
				.ConfigureAwait(false);

			Assert.Null(response.Errors);
			Assert.Equal(name, response.Data.Human.Name);
		}

		[Fact]
		public async void CanDoSerializationWithPredefinedTypes() {
				const string message = "some random testing message";
				var response = await ChatClient.AddMessageAsync(message).ConfigureAwait(false);

				Assert.Equal(message, response.Data.AddMessage.Content);
		}
	}
}
