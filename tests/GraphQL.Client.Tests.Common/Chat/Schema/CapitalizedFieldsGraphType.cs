using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public class CapitalizedFieldsGraphType : ObjectGraphType
{
    public CapitalizedFieldsGraphType()
    {
        Name = "CapitalizedFields";

        Field<StringGraphType>()
            .Name("StringField")
            .Resolve(context => "hello world");
    }
}
