using System.Text.Json;
using GraphQL.Client.Abstractions.Utilities;

namespace GraphQL.Client.Serializer.SystemTextJson;

public class ConstantCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToConstantCase();
}
