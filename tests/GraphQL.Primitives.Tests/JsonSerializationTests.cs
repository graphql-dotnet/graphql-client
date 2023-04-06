using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GraphQL.Primitives.Tests;

public class JsonSerializationTests
{
    [Fact]
    public void WebSocketResponseDeserialization()
    {
        var testObject = new ExtendedTestObject { Id = "test", OtherData = "this is some other stuff" };
        var json = JsonSerializer.Serialize(testObject);
        var deserialized = JsonSerializer.Deserialize<TestObject>(json);
        deserialized.Id.Should().Be("test");
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var childObject = (JsonElement)dict["ChildObject"];
        childObject.GetProperty("Id").GetString().Should().Be(testObject.ChildObject.Id);
    }

    public class TestObject
    {
        public string Id { get; set; }
    }

    public class ExtendedTestObject : TestObject
    {
        public string OtherData { get; set; }

        public TestObject ChildObject { get; set; } = new TestObject { Id = "1337" };
    }
}
