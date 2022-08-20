using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ChatMutation : ObjectGraphType<object>
{
    public ChatMutation(IChat chat)
    {
        Field<MessageType>("addMessage")
            .Argument<MessageInputType>("message")
            .Resolve(context =>
            {
                var receivedMessage = context.GetArgument<ReceivedMessage>("message");
                var message = chat.AddMessage(receivedMessage);
                return message;
            });

        Field<MessageFromType>("join")
            .Argument<StringGraphType>("userId")
            .Resolve(context =>
            {
                var userId = context.GetArgument<string>("userId");
                var userJoined = chat.Join(userId);
                return userJoined;
            });
    }
}

public class MessageInputType : InputObjectGraphType
{
    public MessageInputType()
    {
        Field<StringGraphType>("fromId");
        Field<StringGraphType>("content");
        Field<DateTimeGraphType>("sentAt");
    }
}
