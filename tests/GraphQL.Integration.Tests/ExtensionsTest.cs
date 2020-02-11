using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;

namespace GraphQL.Integration.Tests {
	public class ExtensionsTest {
		private static TestServerSetup SetupTest(bool requestsViaWebsocket = false) =>
			WebHostHelpers.SetupTest<StartupChat>(requestsViaWebsocket);

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
