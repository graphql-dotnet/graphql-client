using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public class ErrorPathConverter : JsonConverter<ErrorPath>
    {

        public override ErrorPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);

            if (doc?.RootElement == null || doc?.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("This converter can only parse when the root element is a JSON Object.");
            }

            return new ErrorPath(ReadArray(doc.RootElement));
        }

        public override void Write(Utf8JsonWriter writer, ErrorPath value, JsonSerializerOptions options)
            => throw new NotImplementedException(
                "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");
        
        private IEnumerable<object?> ReadArray(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                yield return ReadValue(item);
            }
        }

        private object? ReadValue(JsonElement value)
            => value.ValueKind switch
            {
                JsonValueKind.Number => value.ReadNumber(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => throw new InvalidOperationException($"Unexpected value kind: {value.ValueKind}")
            };
    }
}
