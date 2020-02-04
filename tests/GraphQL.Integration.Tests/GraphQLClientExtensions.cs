using System;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Client.Http;

namespace GraphQL.Integration.Tests
{
	public static class GraphQLClientExtensions
	{
		public static async Task<GraphQLResponse> AddMessageAsync(this GraphQLHttpClient client, string message)
		{
			var graphQLRequest = GraphQLRequest.New(
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
			return await client.SendMutationAsync(graphQLRequest).ConfigureAwait(false);
		}

		public static async Task<GraphQLResponse> JoinDeveloperUser(this GraphQLHttpClient client)
		{
			var graphQLRequest = GraphQLRequest.New(@"
				mutation($userId: String){
				  join(userId: $userId){
				    displayName
				    id
				  }
				}",
				new {
					userId = "1"
				});
			return await client.SendMutationAsync(graphQLRequest).ConfigureAwait(false);
		}
	}
}
