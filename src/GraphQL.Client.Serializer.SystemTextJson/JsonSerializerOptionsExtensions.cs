using System.Text.Json;

namespace GraphQL.Client.Serializer.SystemTextJson;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions SetupImmutableConverter(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ImmutableConverter());
        return options;
    }
}
