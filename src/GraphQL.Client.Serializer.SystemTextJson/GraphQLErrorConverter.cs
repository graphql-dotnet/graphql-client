using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson;

/// <summary>
/// A custom converter for the GraphQLError class that allows for deserialization of unknown properties into the Extensions dictionary.
/// </summary>
/// <remarks>
/// This converter respects the known properties of GraphQLError (such as Message, Locations, and Path) and will deserialize unknown properties into the Extensions dictionary.
/// This is particularly useful for GraphQL servers that return additional error information that don't directly map to a property of the GraphQLError class.
/// One such example is GitHub, which returns a "type" field. If we didn't have this converter, the "type" field would be ignored and not accessible in the Extensions dictionary.
/// For more details, see: https://github.com/graphql-dotnet/graphql-client/issues/384
/// </remarks>
public sealed class GraphQLErrorConverter : JsonConverter<GraphQLError>
{
    private static readonly Dictionary<string, PropertyInfo> _knownProps;

    static GraphQLErrorConverter()
    {
        // Cache known GraphQLError properties (case-insensitive)
        _knownProps = typeof(GraphQLError)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Select(p =>
            {
                var dataMember = p.GetCustomAttribute<DataMemberAttribute>();
                var jsonName = dataMember?.Name ?? p.Name;
                return (jsonName, p);
            })
            .ToDictionary(
                x => x.jsonName,
                x => x.p,
                StringComparer.OrdinalIgnoreCase
            );
    }

    public override GraphQLError Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var error = new GraphQLError();
        var unmappedNodes = new Dictionary<string, object>();

        foreach (var prop in root.EnumerateObject())
        {
            if (_knownProps.TryGetValue(prop.Name, out var knownProp))
            {
                var value = JsonSerializer.Deserialize(
                    prop.Value.GetRawText(),
                    knownProp.PropertyType,
                    options
                );

                knownProp.SetValue(error, value);
            }
            else
            {
                unmappedNodes[prop.Name] = ConvertJsonElement(prop.Value);
            }
        }

        if (unmappedNodes.Count > 0 && error.Extensions == null)
            error.Extensions = new Map();

        foreach (var key in unmappedNodes.Keys)
        {
            error.Extensions[key] = unmappedNodes[key];
        }

        return error;
    }

    public override void Write(Utf8JsonWriter writer, GraphQLError value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");
    }

    private static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<object[]>(element.GetRawText()),
            _ => element.GetRawText()
        };
    }
}
