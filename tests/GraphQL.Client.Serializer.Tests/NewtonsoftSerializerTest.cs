using System.Text;
using FluentAssertions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Execution;
using Newtonsoft.Json;
using Xunit;

namespace GraphQL.Client.Serializer.Tests;

[CollectionDefinition(nameof(NewtonsoftDefaultSettingsCollection), DisableParallelization = true)]
public sealed class NewtonsoftDefaultSettingsCollection
{
}

[Collection(nameof(NewtonsoftDefaultSettingsCollection))]
public class NewtonsoftSerializerTest : BaseSerializerTest
{
    public NewtonsoftSerializerTest()
        : base(
            new NewtonsoftJsonSerializer(),
            new NewtonsoftJson.GraphQLSerializer(new ErrorInfoProvider(opt => opt.ExposeData = true)))
    {
    }

    [Fact]
    public void DeserializeToWebsocketResponseRejectsAdditionalJsonContent()
    {
        var serializer = new NewtonsoftJsonSerializer();
        var bytes = Encoding.UTF8.GetBytes(
            """{"id":"1","type":"next","payload":{"value":1}}{"id":"2","type":"next","payload":{"value":2}}""");

        var deserialize = () => serializer.DeserializeToWebsocketResponse<TestPayload>(bytes);

        deserialize.Should().Throw<JsonReaderException>();
    }

    [Fact]
    public void DeserializeToWebsocketResponseUsesJsonConvertDefaultSettings()
    {
        var previousDefaultSettings = JsonConvert.DefaultSettings;
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { FloatParseHandling = FloatParseHandling.Decimal };
            var serializer = new NewtonsoftJsonSerializer();
            var bytes = Encoding.UTF8.GetBytes("""{"id":"1","type":"next","payload":{"value":1.5}}""");

            var response = serializer.DeserializeToWebsocketResponse<TestPayload>(bytes);

            response.Payload.Value.Should().BeOfType<decimal>();
        }
        finally
        {
            JsonConvert.DefaultSettings = previousDefaultSettings;
        }
    }

    private sealed class TestPayload
    {
        public object? Value { get; set; }
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
