using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Serializer.Newtonsoft;

public class MapConverter : JsonConverter<Map>
{
    public override void WriteJson(JsonWriter writer, Map value, JsonSerializer serializer) =>
        throw new NotImplementedException(
            "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

    public override Map ReadJson(JsonReader reader, Type objectType, Map existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var rootToken = JToken.ReadFrom(reader);
        if (rootToken is JObject)
        {
            return (Map)ReadDictionary(rootToken, new Map());
        }
        else
            throw new ArgumentException("This converter can only parse when the root element is a JSON Object.");
    }

    private object? ReadToken(JToken? token) =>
        token switch
        {
            JObject jObject => ReadDictionary(jObject, new Dictionary<string, object>()),
            JArray jArray => ReadArray(jArray).ToList(),
            JValue jValue => jValue.Value,
            JConstructor _ => throw new ArgumentOutOfRangeException(nameof(token.Type),
                "cannot deserialize a JSON constructor"),
            JProperty _ => throw new ArgumentOutOfRangeException(nameof(token.Type),
                "cannot deserialize a JSON property"),
            JContainer _ => throw new ArgumentOutOfRangeException(nameof(token.Type),
                "cannot deserialize a JSON comment"),
            _ => throw new ArgumentOutOfRangeException(nameof(token.Type))
        };

    private Dictionary<string, object> ReadDictionary(JToken element, Dictionary<string, object> to)
    {
        foreach (var property in ((JObject)element).Properties())
        {
            if (IsUnsupportedJTokenType(property.Value.Type))
                continue;
            to[property.Name] = ReadToken(property.Value);
        }
        return to;
    }

    private IEnumerable<object?> ReadArray(JArray element)
    {
        foreach (var item in element)
        {
            if (IsUnsupportedJTokenType(item.Type))
                continue;
            yield return ReadToken(item);
        }
    }

    private bool IsUnsupportedJTokenType(JTokenType type) => type == JTokenType.Constructor || type == JTokenType.Property || type == JTokenType.Comment;
}
