using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ChatSchema : Types.Schema
{
    public ChatSchema(IServiceProvider services)
        : base(services)
    {
        Query = services.GetRequiredService<ChatQuery>();
        Mutation = services.GetRequiredService<ChatMutation>();
        Subscription = services.GetRequiredService<ChatSubscriptions>();
    }
}
