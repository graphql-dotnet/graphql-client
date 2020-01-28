using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema
{
    public class ChatSchema : Schema
    {
        public ChatSchema(IChat chat) 
        {
            Query = new ChatQuery(chat);
            Mutation = new ChatMutation(chat);
            Subscription = new ChatSubscriptions(chat);
        }
    }
}
