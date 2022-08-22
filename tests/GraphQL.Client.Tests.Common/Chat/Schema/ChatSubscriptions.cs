using System.Reactive.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ChatSubscriptions : ObjectGraphType<object>
{
    private readonly IChat _chat;

    public ChatSubscriptions(IChat chat)
    {
        _chat = chat;
        AddField(new FieldType
        {
            Name = "messageAdded",
            Type = typeof(MessageType),
            Resolver = new FuncFieldResolver<Message>(ResolveMessage),
            StreamResolver = new SourceStreamResolver<Message>(Subscribe)
        });

        AddField(new FieldType
        {
            Name = "contentAdded",
            Type = typeof(MessageType),
            Resolver = new FuncFieldResolver<Message>(ResolveMessage),
            StreamResolver = new SourceStreamResolver<Message>(Subscribe)
        });

        AddField(new FieldType
        {
            Name = "messageAddedByUser",
            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }
            ),
            Type = typeof(MessageType),
            Resolver = new FuncFieldResolver<Message>(ResolveMessage),
            StreamResolver = new SourceStreamResolver<Message>(SubscribeById)
        });

        AddField(new FieldType
        {
            Name = "userJoined",
            Type = typeof(MessageFromType),
            Resolver = new FuncFieldResolver<MessageFrom>(context => context.Source as MessageFrom),
            StreamResolver = new SourceStreamResolver<MessageFrom>(context => _chat.UserJoined())
        });


        AddField(new FieldType
        {
            Name = "failImmediately",
            Type = typeof(MessageType),
            Resolver = new FuncFieldResolver<Message>(ResolveMessage),
            StreamResolver = new SourceStreamResolver<Message>((Func<IResolveFieldContext, IObservable<Message>>)(context => throw new NotSupportedException("this is supposed to fail")))
        });
    }

    private IObservable<Message> SubscribeById(IResolveFieldContext context)
    {
        var user = context.User;

        var sub = "Anonymous";
        if (user != null)
            sub = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        var messages = _chat.Messages(sub);

        var id = context.GetArgument<string>("id");
        return messages.Where(message => message.From.Id == id);
    }

    private Message ResolveMessage(IResolveFieldContext context)
    {
        var message = context.Source as Message;

        return message;
    }

    private IObservable<Message> Subscribe(IResolveFieldContext context)
    {
        var user = context.User;

        var sub = "Anonymous";
        if (user != null)
            sub = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        return _chat.Messages(sub);
    }
}
