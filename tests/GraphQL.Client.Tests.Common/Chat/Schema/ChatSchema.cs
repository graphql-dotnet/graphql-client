namespace GraphQL.Client.Tests.Common.Chat.Schema
{
    public class ChatSchema : Types.Schema
    {
        public ChatSchema(IDependencyResolver resolver)
            : base(resolver)
        {
            Query = resolver.Resolve<ChatQuery>();
            Mutation = resolver.Resolve<ChatMutation>();
            Subscription = resolver.Resolve<ChatSubscriptions>();
        }
    }
}
