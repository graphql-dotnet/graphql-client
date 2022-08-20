namespace GraphQL.Client.Tests.Common.Chat;

public class AddMessageMutationResult
{
    public AddMessageContent AddMessage { get; set; }

    public class AddMessageContent
    {
        public string Content { get; set; }
    }
}
