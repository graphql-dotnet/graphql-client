using System.Text.Json;
using Panic.StringUtils.Extensions;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public class ConstantCaseJsonNamingPolicy: JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToConstantCase();
    }
}
