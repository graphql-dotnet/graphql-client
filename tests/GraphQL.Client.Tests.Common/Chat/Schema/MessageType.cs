using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class MessageType : ObjectGraphType<Message>
{
    public MessageType()
    {
        Field(o => o.Content);
        Field(o => o.SentAt);
        Field(o => o.Sub);
        Field(o => o.From, false, typeof(MessageFromType)).Resolve(ResolveFrom);
    }

    private MessageFrom ResolveFrom(IResolveFieldContext<Message> context)
    {
        var message = context.Source;
        return message.From;
    }
}
