using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.StarWars.Types;

public class HumanInputType : InputObjectGraphType
{
    public HumanInputType()
    {
        Name = "HumanInput";
        Field<NonNullGraphType<StringGraphType>>("name");
        Field<StringGraphType>("homePlanet");
    }
}
