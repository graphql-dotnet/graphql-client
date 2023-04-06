using FluentAssertions;
using FluentAssertions.Execution;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Serializer.SystemTextJson;
using Newtonsoft.Json;
using Xunit;

namespace GraphQL.Client.Serializer.Tests;

public class ConsistencyTests
{
    [Fact]
    public void MapConvertersShouldBehaveConsistent()
    {
        const string json = @"{
                ""array"": [
                  ""some stuff"",
                  ""something else""
                ],
                ""string"": ""this is a string"",
                ""boolean"": true,
                ""number"": 1234.567,
                ""nested object"": {
                    ""prop1"": false
                },
                ""arrayOfObjects"": [
                  {""number"": 1234.567},
                  {""number"": 567.8}
                ]
            }";

        var newtonsoftSerializer = new NewtonsoftJsonSerializer();
        var systemTextJsonSerializer = new SystemTextJsonSerializer();

        var newtonsoftMap = JsonConvert.DeserializeObject<Map>(json, newtonsoftSerializer.JsonSerializerSettings);
        var systemTextJsonMap = System.Text.Json.JsonSerializer.Deserialize<Map>(json, systemTextJsonSerializer.Options);


        using (new AssertionScope())
        {
            CompareMaps(newtonsoftMap, systemTextJsonMap);
        }

        newtonsoftMap.Should().BeEquivalentTo(systemTextJsonMap, options => options
            .RespectingRuntimeTypes());
    }

    private void CompareMaps(Dictionary<string, object> first, Dictionary<string, object> second)
    {
        foreach (var keyValuePair in first)
        {
            second.Should().ContainKey(keyValuePair.Key);
            second[keyValuePair.Key].Should().BeOfType(keyValuePair.Value.GetType());
            if (keyValuePair.Value is Dictionary<string, object> map)
                CompareMaps(map, (Dictionary<string, object>)second[keyValuePair.Key]);
            else
                keyValuePair.Value.Should().BeEquivalentTo(second[keyValuePair.Key]);
        }
    }
}
