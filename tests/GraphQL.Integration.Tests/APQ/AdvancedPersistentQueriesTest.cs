using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Tests.Common.StarWars.TestData;
using GraphQL.Integration.Tests.Helpers;
using Xunit;

namespace GraphQL.Integration.Tests.APQ;

[SuppressMessage("ReSharper", "UseConfigureAwaitFalse")]
public class AdvancedPersistentQueriesTest : IAsyncLifetime, IClassFixture<SystemTextJsonAutoNegotiateServerTestFixture>
{
    public SystemTextJsonAutoNegotiateServerTestFixture Fixture { get; }
    protected GraphQLHttpClient StarWarsClient;
    protected GraphQLHttpClient StarWarsWebsocketClient;

    public AdvancedPersistentQueriesTest(SystemTextJsonAutoNegotiateServerTestFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await Fixture.CreateServer();
        StarWarsClient = Fixture.GetStarWarsClient(options => options.EnableAutomaticPersistedQueries = _ => true);
        StarWarsWebsocketClient = Fixture.GetStarWarsClient(options =>
        {
            options.EnableAutomaticPersistedQueries = _ => true;
            options.UseWebSocketForQueriesAndMutations = true;
        });
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

    [Theory]
    [ClassData(typeof(StarWarsHumans))]
    public async void After_querying_all_starwars_humans_using_websocket_transport_the_APQDisabledForSession_is_still_false_Async(int id, string name)
    {
        var query = new GraphQLQuery("""
                                     query Human($id: String!){
                                     human(id: $id) {
                                             name
                                         }
                                     }
                                     """);

        var graphQLRequest = new GraphQLRequest(query, new { id = id.ToString() });

        var response = await StarWarsWebsocketClient.SendQueryAsync(graphQLRequest, () => new { Human = new { Name = string.Empty } });

        Assert.Null(response.Errors);
        Assert.Equal(name, response.Data.Human.Name);
        StarWarsWebsocketClient.APQDisabledForSession.Should().BeFalse("if APQ has worked it won't get disabled");
    }

    [Fact]
    public void Verify_the_persisted_query_extension_object()
    {
        var query = new GraphQLQuery("""
                                     query Human($id: String!){
                                     human(id: $id) {
                                             name
                                         }
                                     }
                                     """);
        query.Sha256Hash.Should().NotBeNullOrEmpty();

        var request = new GraphQLRequest(query);
        request.Extensions.Should().BeNull();
        request.GeneratePersistedQueryExtension();
        request.Extensions.Should().NotBeNull();

        string expectedKey = "persistedQuery";
        var expectedExtensionValue = new Dictionary<string, object>
        {
            ["version"] = 1,
            ["sha256Hash"] = query.Sha256Hash,
        };

        request.Extensions.Should().ContainKey(expectedKey);
        request.Extensions![expectedKey].As<Dictionary<string, object>>()
            .Should().NotBeNull().And.BeEquivalentTo(expectedExtensionValue);
    }
}
