using GraphQL.Server.Test.GraphQL.Models;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL;

public class TestQuery : ObjectGraphType
{
    public TestQuery()
    {
        Field<RepositoryGraphType>("repository")
            .Argument<NonNullGraphType<StringGraphType>>("owner")
            .Argument<NonNullGraphType<StringGraphType>>("name")
            .Resolve(context =>
        {
            var owner = context.GetArgument<string>("owner");
            var name = context.GetArgument<string>("name");
            return Storage.Repositories.FirstOrDefault(predicate => predicate.Name == name);
        });
    }
}
