using System;
using System.Text.Json;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using IntegrationTestServer.ChatSchema;
using Newtonsoft.Json;
using Xunit;

namespace GraphQL.Integration.Tests {
	public class ExtensionsTest {
		private static TestServerSetup SetupTest(bool requestsViaWebsocket = false) =>
			WebHostHelpers.SetupTest<StartupChat>(requestsViaWebsocket);

		//[Fact]
		//public async void CanDeserializeExtensions() {

		//	using var setup = SetupTest();
		//	var response = await setup.Client.SendQueryAsync(new GraphQLRequest("query { extensionsTest }"),
		//			() => new {extensionsTest = ""})
		//		.ConfigureAwait(false);

		//	response.Errors.Should().NotBeNull();
		//	response.Errors.Should().ContainSingle();
		//	response.Errors[0].Extensions.Should().NotBeNull();
		//	response.Errors[0].Extensions.Should().ContainKey("data");

		//	foreach (var item in ChatQuery.TestExtensions) {
				

		//	}
		//}

		//[Fact]
		//public async void DontNeedToUseCamelCaseNamingStrategy() {

		//	using var setup = SetupTest();
		//	setup.Client.Options.JsonSerializerSettings = new JsonSerializerSettings();

		//	const string message = "some random testing message";
		//	var graphQLRequest = new GraphQLRequest(
		//		@"mutation($input: MessageInputType){
		//		  addMessage(message: $input){
		//		    content
		//		  }
		//		}",
		//		new {
		//			input = new {
		//				fromId = "2",
		//				content = message,
		//				sentAt = DateTime.Now
		//			}
		//		});
		//	var response = await setup.Client.SendMutationAsync(graphQLRequest, () => new { addMessage = new { content = "" } });

		//	Assert.Equal(message, response.Data.addMessage.content);
		//}
	}
}
