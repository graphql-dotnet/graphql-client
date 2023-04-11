using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests;

public class Newtonsoft : Base, IClassFixture<NewtonsoftGraphQLWsServerTestFixture>
{
    public Newtonsoft(NewtonsoftGraphQLWsServerTestFixture fixture) : base(fixture)
    {
    }
}
