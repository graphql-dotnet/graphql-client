using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Execution;

namespace GraphQL.Client.Serializer.Tests;

public class SystemTextJsonSerializerTests : BaseSerializerTest
{
    public SystemTextJsonSerializerTests()
        : base(
            new SystemTextJsonSerializer(),
            new GraphQL.SystemTextJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }
}

public class SystemTextJsonSerializeNoCamelCaseTest : BaseSerializeNoCamelCaseTest
{
    public SystemTextJsonSerializeNoCamelCaseTest()
        : base(
            new SystemTextJsonSerializer(new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(new ConstantCaseJsonNamingPolicy(), false) } }.SetupImmutableConverter()),
            new GraphQL.SystemTextJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }
}
