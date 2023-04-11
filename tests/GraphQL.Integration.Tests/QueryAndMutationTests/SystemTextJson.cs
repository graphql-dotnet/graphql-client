using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests;

public class SystemTextJson : Base, IClassFixture<SystemTextJsonGraphQLWsServerTestFixture>
{
    public SystemTextJson(SystemTextJsonGraphQLWsServerTestFixture fixture) : base(fixture)
    {
    }
}
