using System;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Integration.Tests
{
	public static class GraphQLClientExtensions
	{
		public static async Task<GraphQLResponse> AddMessageAsync(this GraphQLHttpClient client, string message)
		{
			var graphQLRequest = new GraphQLRequest
			{
				Query = @"
				mutation($input: MessageInputType){
				  addMessage(message: $input){
				    content
				  }
				}",
				Variables = new
				{
					input = new
					{
						fromId = "me",
						content = "Lorem ipsum dolor si amet",
						sentAt = DateTime.Now
					}
				}
			};
			return await client.SendMutationAsync(graphQLRequest).ConfigureAwait(false);
		}
	}
}
