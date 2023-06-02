using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Execution;
using Xunit;

namespace GraphQL.Client.Serializer.Tests;

public class SystemTextJsonSerializerTests : BaseSerializerTest
{
    public SystemTextJsonSerializerTests()
        : base(
            new SystemTextJsonSerializer(),
            new GraphQL.SystemTextJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }

    [Fact]
    public async Task DeserializingObjectWithBothConstructorAndProperties()
    {
        // Arrange
        const string jsonString = @"{ ""data"": { ""optionalTestProperty"": ""optional"", ""requiredTestProperty"": ""required"" } }";
        var graphQlSerializer = new SystemTextJsonSerializer();
        var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
        
        // Act
        var result = await graphQlSerializer.DeserializeFromUtf8StreamAsync<SerializerTestClass>(contentStream, default).ConfigureAwait(false);
        
        // Assert
        result.Data.RequiredTestProperty.Should().Be("required");
        result.Data.OptionalTestProperty.Should().Be("optional");
    }

    private class SerializerTestClass
    {
        public SerializerTestClass(string requiredTestProperty)
        {
            RequiredTestProperty = requiredTestProperty;
        }
        public string? OptionalTestProperty { get; set; }
        public string RequiredTestProperty { get; }
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
