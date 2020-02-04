using System;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Client.Http;

namespace GraphQL.Integration.Tests {
	public static class GraphQLClientTestExtensions {
		public static Task<GraphQLResponse<AddMessageMutationResult>> AddMessageAsync(this GraphQLHttpClient client, string message) {
			var graphQLRequest = new GraphQLRequest(
				@"mutation($input: MessageInputType){
				  addMessage(message: $input){
				    content
				  }
				}",
				new {
					input = new {
						fromId = "2",
						content = message,
						sentAt = DateTime.Now
					}
				});
			return client.SendMutationAsync<AddMessageMutationResult>(graphQLRequest);
		}

		public static Task<GraphQLResponse<JoinDeveloperMutationResult>> JoinDeveloperUser(this GraphQLHttpClient client) {
			var graphQLRequest = new GraphQLRequest(@"
				mutation($userId: String){
				  join(userId: $userId){
				    displayName
				    id
				  }
				}",
				new {
					userId = "1"
				});
			return client.SendMutationAsync<JoinDeveloperMutationResult>(graphQLRequest);
		}

	}


	public class AddMessageMutationResult {
		public AddMessageContent AddMessage { get; set; }
		public class AddMessageContent {
			public string Content { get; set; }
		}
	}

	public class JoinDeveloperMutationResult {
		public JoinContent Join { get; set; }
		public class JoinContent {
			public string DisplayName { get; set; }
			public string Id { get; set; }
		}
	}
}
