using System.Linq;
using GraphQL.Server.Test.GraphQL.Models;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL
{

    public class TestQuery : ObjectGraphType
    {

        public TestQuery()
        {
            this.Field<RepositoryGraphType>("repository", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "owner" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: context =>
            {
                var owner = context.GetArgument<string>("owner");
                var name = context.GetArgument<string>("name");
                return Storage.Repositories.FirstOrDefault(predicate => predicate.Name == name);
            });
        }

    }

}
