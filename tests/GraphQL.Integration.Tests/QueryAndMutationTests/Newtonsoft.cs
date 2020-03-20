using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.QueryAndMutationTests
{
    public class Newtonsoft : Base, IClassFixture<NewtonsoftIntegrationServerTestFixture>
    {
        public Newtonsoft(NewtonsoftIntegrationServerTestFixture fixture) : base(fixture)
        {
        }
    }
}
