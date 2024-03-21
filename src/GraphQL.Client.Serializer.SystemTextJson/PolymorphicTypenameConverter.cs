using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public abstract class PolymorphicTypenameConverter<T> : JsonConverter<T>
    {
        protected abstract Type GetTypeFromTypenameField(string typename);

        public override bool CanConvert(Type typeToConvert) =>
            typeof(T).IsAssignableFrom(typeToConvert);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var clone = reader; // cause its a struct

            if (clone.TokenType == JsonTokenType.StartObject)
                clone.Read();
            if (clone.TokenType != JsonTokenType.PropertyName || clone.GetString() != "__typename")

                throw new JsonException();

            clone.Read();
            var type = Descriminator(clone.GetString());
            object deserialize = JsonSerializer.Deserialize(ref reader, type, options);
            return (T)deserialize;
        }

        public override void Write(Utf8JsonWriter writer, T obj, JsonSerializerOptions options) =>
            throw new NotSupportedException();
    }
}
