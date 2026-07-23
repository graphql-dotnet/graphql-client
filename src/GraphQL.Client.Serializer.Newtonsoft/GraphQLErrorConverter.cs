using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Serializer.Newtonsoft;

/// <summary>
/// A custom converter for the GraphQLError class that allows for deserialization of unknown properties into the Extensions dictionary.
/// </summary>
/// <remarks>
/// This converter respects the known properties of GraphQLError (such as Message, Locations, and Path) and will deserialize unknown properties into the Extensions dictionary.
/// This is particularly useful for GraphQL servers that return additional error information that don't directly map to a property of the GraphQLError class.
/// One such example is GitHub, which returns a "type" field. If we didn't have this converter, the "type" field would be ignored and not accessible in the Extensions dictionary.
/// For more details, see: https://github.com/graphql-dotnet/graphql-client/issues/384
/// </remarks>
public sealed class GraphQLErrorConverter : JsonConverter
{
    private static readonly Dictionary<string, PropertyInfo> _knownProps;

    static GraphQLErrorConverter()
    {
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

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(GraphQLError);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);

        var error = new GraphQLError();
        var unmappedNodes = new Dictionary<string, object>();

        foreach (var prop in jo.Properties())
        {
            if (_knownProps.TryGetValue(prop.Name, out var knownProp))
            {
                var value = prop.Value.ToObject(knownProp.PropertyType, serializer);
                knownProp.SetValue(error, value);
            }
            else
            {
                unmappedNodes[prop.Name] = ConvertJToken(prop.Value);
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

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Writing GraphQLError is not required.");
    }

    private static object ConvertJToken(JToken token)
    {
        return token.Type switch
        {
            JTokenType.String => token.ToObject<string>(),
            JTokenType.Integer => token.ToObject<long>(),
            JTokenType.Float => token.ToObject<double>(),
            JTokenType.Boolean => token.ToObject<bool>(),
            JTokenType.Object => token.ToObject<Dictionary<string, object>>(),
            JTokenType.Array => token.ToObject<object[]>(),
            _ => token.ToString()
        };
    }
}
