using GraphQL.Client.Abstractions;

namespace GraphQL.Client.Tests.Common.Chat;

public static class GraphQLClientChatExtensions
{
    public const string ADD_MESSAGE_QUERY =
@"mutation($input: MessageInputType){
  addMessage(message: $input){
    content
  }
}";

    public static Task<GraphQLResponse<AddMessageMutationResult>> AddMessageAsync(this IGraphQLClient client, string message)
    {
        var variables = new AddMessageVariables
        {
            Input = new AddMessageVariables.AddMessageInput
            {
                FromId = "2",
                Content = message,
                SentAt = DateTime.Now.ToString("s")
            }
        };

        var graphQLRequest = new GraphQLRequest(ADD_MESSAGE_QUERY, variables);
        return client.SendMutationAsync<AddMessageMutationResult>(graphQLRequest);
    }

    public static Task<GraphQLResponse<JoinDeveloperMutationResult>> JoinDeveloperUser(this IGraphQLClient client)
    {
        var graphQLRequest = new GraphQLRequest(@"
				mutation($userId: String){
				  join(userId: $userId){
				    displayName
				    id
				  }
				}",
            new
            {
                userId = "1"
            });
        return client.SendMutationAsync<JoinDeveloperMutationResult>(graphQLRequest);
    }
}
