using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Execution;
using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Tests;

public class NewtonsoftSerializerTest : BaseSerializerTest
{
    public NewtonsoftSerializerTest()
        : base(
            new NewtonsoftJsonSerializer(),
            new NewtonsoftJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }
}

public class NewtonsoftSerializeNoCamelCaseTest : BaseSerializeNoCamelCaseTest
{
    public NewtonsoftSerializeNoCamelCaseTest()
        : base(
            new NewtonsoftJsonSerializer(new JsonSerializerSettings { Converters = { new ConstantCaseEnumConverter() } }),
            new NewtonsoftJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }
}
