namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class ReceivedMessage
{
    public string FromId { get; set; }

    public string Content { get; set; }

    public DateTime SentAt { get; set; }
}
