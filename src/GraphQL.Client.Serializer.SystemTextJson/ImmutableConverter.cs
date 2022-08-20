using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson;

/// <summary>
/// class for converting immutable objects, derived from https://github.com/manne/obviously/blob/master/src/system.text.json/Core/ImmutableConverter.cs
/// </summary>
public class ImmutableConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsPrimitive)
            return false;

        var nullableUnderlyingType = Nullable.GetUnderlyingType(typeToConvert);
        if (nullableUnderlyingType != null && nullableUnderlyingType.IsValueType)
            return false;

        bool result;
        var constructors = typeToConvert.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (constructors.Length != 1)
        {
            result = false;
        }
        else
        {
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();

            if (parameters.Length > 0)
            {
                var properties = typeToConvert.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                result = parameters
                    .Select(parameter => properties.Any(p => NameOfPropertyAndParameter.Matches(p.Name, parameter.Name, typeToConvert.IsAnonymous())))
                    .All(hasMatchingProperty => hasMatchingProperty);
            }
            else
            {
                result = false;
            }
        }

        return result;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueOfProperty = new Dictionary<PropertyInfo, object>();
        var namedPropertiesMapping = GetNamedProperties(options, GetProperties(typeToConvert));
        reader.Read();
        while (true)
        {
            if (reader.TokenType != JsonTokenType.PropertyName && reader.TokenType != JsonTokenType.String)
            {
                break;
            }

            string jsonPropName = reader.GetString();
            string normalizedPropName = ConvertAndNormalizeName(jsonPropName, options);
            if (!namedPropertiesMapping.TryGetValue(normalizedPropName, out var obProp))
            {
                reader.Read();
            }
            else
            {
                var value = JsonSerializer.Deserialize(ref reader, obProp.PropertyType, options);
                reader.Read();
                valueOfProperty[obProp] = value;
            }
        }

        var ctor = typeToConvert.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
        var parameters = ctor.GetParameters();
        var parameterValues = new object[parameters.Length];
        for (int index = 0; index < parameters.Length; index++)
        {
            var parameterInfo = parameters[index];
            var value = valueOfProperty.First(prop =>
                                                  NameOfPropertyAndParameter.Matches(prop.Key.Name, parameterInfo.Name, typeToConvert.IsAnonymous())).Value;

            parameterValues[index] = value;
        }

        var instance = ctor.Invoke(parameterValues);
        return instance;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var strippedOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            DefaultBufferSize = options.DefaultBufferSize,
            DictionaryKeyPolicy = options.DictionaryKeyPolicy,
            Encoder = options.Encoder,
            IgnoreNullValues = options.IgnoreNullValues,
            IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties,
            MaxDepth = options.MaxDepth,
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
            PropertyNamingPolicy = options.PropertyNamingPolicy,
            ReadCommentHandling = options.ReadCommentHandling,
            WriteIndented = options.WriteIndented
        };
        foreach (var converter in options.Converters)
        {
            if (!(converter is ImmutableConverter))
                strippedOptions.Converters.Add(converter);
        }

        JsonSerializer.Serialize(writer, value, strippedOptions);
    }

    private static PropertyInfo[] GetProperties(IReflect typeToConvert) => typeToConvert.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static IReadOnlyDictionary<string, PropertyInfo> GetNamedProperties(JsonSerializerOptions options, IEnumerable<PropertyInfo> properties)
    {
        var result = new Dictionary<string, PropertyInfo>();
        foreach (var property in properties)
        {
            string name;
            var nameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (nameAttribute != null)
            {
                name = nameAttribute.Name;
            }
            else
            {
                name = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
            }

            string normalizedName = NormalizeName(name, options);
            result.Add(normalizedName, property);
        }

        return result;
    }

    private static string ConvertAndNormalizeName(string name, JsonSerializerOptions options)
    {
        string convertedName = options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        return NormalizeName(convertedName, options);
    }

    private static string NormalizeName(string name, JsonSerializerOptions options) => options.PropertyNameCaseInsensitive ? name.ToLowerInvariant() : name;
}

internal static class NameOfPropertyAndParameter
{
    public static bool Matches(string propertyName, string parameterName, bool anonymousType)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return string.IsNullOrEmpty(parameterName);
        }

        if (string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        if (anonymousType)
        {
            return propertyName.Equals(parameterName, StringComparison.Ordinal);
        }

        var xRight = propertyName.AsSpan(1);
        var yRight = parameterName.AsSpan(1);
        return char.ToLowerInvariant(propertyName[0]).CompareTo(parameterName[0]) == 0 && xRight.Equals(yRight, StringComparison.Ordinal);
    }
}

internal static class TypeExtensions
{
    // copied from https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json/Util/TypeExtensions.cs
    public static bool IsAnonymous(this Type type) =>
        type.Namespace == null
        && type.IsSealed
        && type.BaseType == typeof(object)
        && !type.IsPublic
        && type.IsDefined(typeof(CompilerGeneratedAttribute), false);
}
