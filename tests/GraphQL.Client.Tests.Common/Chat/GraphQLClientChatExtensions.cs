using System;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;

namespace GraphQL.Client.Tests.Common.Chat {
	public static class GraphQLClientChatExtensions {
		public static Task<GraphQLResponse<AddMessageMutationResult>> AddMessageAsync(this IGraphQLClient client, string message) {
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

		public static Task<GraphQLResponse<JoinDeveloperMutationResult>> JoinDeveloperUser(this IGraphQLClient client) {
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
}
