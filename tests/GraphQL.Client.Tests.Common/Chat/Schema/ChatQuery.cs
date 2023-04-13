using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ChatQuery : ObjectGraphType
{
    private readonly IServiceProvider _serviceProvider;

    public static readonly Dictionary<string, object> TestExtensions = new()
    {
        {"extension1", "hello world"},
        {"another extension", 4711},
        {"long", 19942590700}
    };

    // properties for unit testing

    public readonly ManualResetEventSlim LongRunningQueryBlocker = new ManualResetEventSlim();
    public readonly ManualResetEventSlim WaitingOnQueryBlocker = new ManualResetEventSlim();

    public ChatQuery(IChat chat, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

        Field<StringGraphType>("clientUserAgent")
            .Resolve(context =>
            {
                var contextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
                if (!contextAccessor.HttpContext.Request.Headers.UserAgent.Any())
                {
                    context.Errors.Add(new ExecutionError("user agent header not set"));
                    return null;
                }
                return contextAccessor.HttpContext.Request.Headers.UserAgent.ToString();
            });
    }
}
