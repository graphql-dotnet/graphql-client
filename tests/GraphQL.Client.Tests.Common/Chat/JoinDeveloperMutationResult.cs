namespace GraphQL.Client.Tests.Common.Chat;

public class JoinDeveloperMutationResult
{
    public JoinContent Join { get; set; }

    public class JoinContent
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }
    }
}
