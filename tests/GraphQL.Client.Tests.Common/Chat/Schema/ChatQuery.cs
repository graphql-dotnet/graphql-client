using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ChatQuery : ObjectGraphType
{
    public static readonly Dictionary<string, object> TestExtensions = new()
    {
        {"extension1", "hello world"},
        {"another extension", 4711},
        {"long", 19942590700}
    };

    // properties for unit testing

    public readonly ManualResetEventSlim LongRunningQueryBlocker = new ManualResetEventSlim();
    public readonly ManualResetEventSlim WaitingOnQueryBlocker = new ManualResetEventSlim();

    public ChatQuery(IChat chat)
    {
        Name = "ChatQuery";

        Field<ListGraphType<MessageType>>("messages").Resolve(context => chat.AllMessages.Take(100));

        Field<StringGraphType>("extensionsTest")
            .Resolve(context =>
            {
                context.Errors.Add(new ExecutionError("this error contains extension fields", TestExtensions));
                return null;
            });

        Field<StringGraphType>("longRunning")
            .Resolve(context =>
            {
                WaitingOnQueryBlocker.Set();
                LongRunningQueryBlocker.Wait();
                WaitingOnQueryBlocker.Reset();
                return "finally returned";
            });
    }
}
