using GraphQL.Server.Test.GraphQL.Models;

namespace GraphQL.Server.Test.GraphQL;

public static class Storage
{
    public static IQueryable<Repository> Repositories { get; } = new List<Repository>()
        .Append(new Repository
        {
            DatabaseId = 113196300,
            Id = "MDEwOlJlcG9zaXRvcnkxMTMxOTYzMDA=",
            Name = "graphql-client",
            Owner = null,
            Url = new Uri("https://github.com/graphql-dotnet/graphql-client")
        })
        .AsQueryable();
}
