using System.Linq;
using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema
{
    public class ChatQuery : ObjectGraphType
    {
        public ChatQuery(IChat chat)
        {
            Field<ListGraphType<MessageType>>("messages", resolve: context => chat.AllMessages.Take(100));
        }
    }
}
