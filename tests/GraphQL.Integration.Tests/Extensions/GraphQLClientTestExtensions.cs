using System;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.Chat;

namespace GraphQL.Integration.Tests.Extensions {
	public static class GraphQLClientTestExtensions {
		

		public static Task<GraphQLResponse<AddMessageMutationResult>> AddMessageAsync(this GraphQLHttpClient client, string message) {
			var variables = new AddMessageVariables {
				Input = new AddMessageVariables.AddMessageInput{
					FromId = "2",
					Content = message,
					SentAt = DateTime.Now
				}
			};

			var graphQLRequest = new GraphQLRequest(
				@"mutation($input: MessageInputType){
				  addMessage(message: $input){
				    content
				  }
				}",
				variables);
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
