namespace GraphQL.Client.Tests.Common.Chat;

public class AddMessageVariables
{
    public AddMessageInput Input { get; set; }

    public class AddMessageInput
    {
        public string FromId { get; set; }

        public string Content { get; set; }

        public string SentAt { get; set; }
    }
}
