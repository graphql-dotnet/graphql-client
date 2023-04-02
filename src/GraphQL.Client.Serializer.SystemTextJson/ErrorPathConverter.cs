using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson;

public class ErrorPathConverter : JsonConverter<ErrorPath>
{

    public override ErrorPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(ReadArray(ref reader));

    public override void Write(Utf8JsonWriter writer, ErrorPath value, JsonSerializerOptions options)
        => throw new NotImplementedException(
            "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

    private static IEnumerable<object?> ReadArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("This converter can only parse when the root element is a JSON Array.");
        }

        var array = new List<object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            array.Add(ReadValue(ref reader));
        }

        return array;
    }

    private static object? ReadValue(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.None => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.ReadNumber(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            _ => throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}")
        };
}
