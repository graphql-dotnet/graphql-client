using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson;

/// <summary>
/// A custom JsonConverter for reading the extension fields of <see cref="GraphQLResponse{T}"/> and <see cref="GraphQLError"/>.
/// </summary>
/// <remarks>
/// Taken and modified from GraphQL.SystemTextJson.ObjectDictionaryConverter (GraphQL.NET)
/// </remarks>
public class MapConverter : JsonConverter<Map>
{
    public override Map Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ReadDictionary(ref reader, new Map());

    public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
        => throw new NotImplementedException(
            "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

    private static TDictionary ReadDictionary<TDictionary>(ref Utf8JsonReader reader, TDictionary result)
        where TDictionary : Dictionary<string, object>
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string key = reader.GetString();

            // move to property value
            if (!reader.Read())
                throw new JsonException();

            result.Add(key, ReadValue(ref reader));
        }

        return result;
    }

    private static List<object> ReadArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var result = new List<object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            result.Add(ReadValue(ref reader));
        }

        return result;
    }

    private static object? ReadValue(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.StartArray => ReadArray(ref reader).ToList(),
            JsonTokenType.StartObject => ReadDictionary(ref reader, new Dictionary<string, object>()),
            JsonTokenType.Number => reader.ReadNumber(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            JsonTokenType.None => null,
            _ => throw new InvalidOperationException($"Unexpected value kind: {reader.TokenType}")
        };
}
