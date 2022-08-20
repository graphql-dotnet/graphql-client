using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models;

public class Repository
{
    public int DatabaseId { get; set; }

    public string? Id { get; set; }

    public string? Name { get; set; }

    public object? Owner { get; set; }

    public Uri? Url { get; set; }
}

public class RepositoryGraphType : ObjectGraphType<Repository>
{
    public RepositoryGraphType()
    {
        Name = nameof(Repository);
        Field(expression => expression.DatabaseId);
        Field<NonNullGraphType<IdGraphType>>("id");
        Field(expression => expression.Name);
        //this.Field(expression => expression.Owner);
        Field<NonNullGraphType<UriGraphType>>("url");
    }
}
