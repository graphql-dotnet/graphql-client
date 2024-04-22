using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.StarWars.TestData;
using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.APQ;

[SuppressMessage("ReSharper", "UseConfigureAwaitFalse")]
public class APQViaHttpRequests : IAsyncLifetime, IClassFixture<SystemTextJsonAutoNegotiateServerTestFixture>
{
    public SystemTextJsonAutoNegotiateServerTestFixture Fixture { get; }
    protected GraphQLHttpClient StarWarsClient;

    public APQViaHttpRequests(SystemTextJsonAutoNegotiateServerTestFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await Fixture.CreateServer();
        StarWarsClient = Fixture.GetStarWarsClient(options => options.EnableAutomaticPersistedQueries = _ => true);
    }

    public Task DisposeAsync()
    {
        StarWarsClient?.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [ClassData(typeof(StarWarsHumans))]
    public async void After_querying_all_starwars_humans_the_APQDisabledForSession_is_still_false_Async(int id, string name)
    {
        var query = new GraphQLQuery("""
                                     query Human($id: String!){
                                     human(id: $id) {
                                             name
                                         }
                                     }
                                     """);

        var graphQLRequest = new GraphQLRequest(query, new { id = id.ToString() });

        var response = await StarWarsClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });

        Assert.Null(response.Errors);
        Assert.Equal(name, response.Data.Human.Name);
        StarWarsClient.APQDisabledForSession.Should().BeFalse("if APQ has worked it won't get disabled");
    }
}
