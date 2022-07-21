using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class MessageFromType : ObjectGraphType<MessageFrom>
{
    public MessageFromType()
    {
        Field(o => o.Id);
        Field(o => o.DisplayName);
    }
}
