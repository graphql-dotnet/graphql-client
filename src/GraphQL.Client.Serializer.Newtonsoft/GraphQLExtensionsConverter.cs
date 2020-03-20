using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Serializer.Newtonsoft
{
    public class GraphQLExtensionsConverter : JsonConverter<GraphQLExtensionsType>
    {
        public override void WriteJson(JsonWriter writer, GraphQLExtensionsType value, JsonSerializer serializer) =>
            throw new NotImplementedException(
                "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

        public override GraphQLExtensionsType ReadJson(JsonReader reader, Type objectType, GraphQLExtensionsType existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var rootToken = JToken.ReadFrom(reader);
            if (rootToken is JObject)
            {
                return ReadDictionary<GraphQLExtensionsType>(rootToken);
            }
            else
                throw new ArgumentException("This converter can only parse when the root element is a JSON Object.");
        }

        private object ReadToken(JToken? token) =>
            token.Type switch
            {
                JTokenType.Undefined => null,
                JTokenType.None => null,
                JTokenType.Null => null,
                JTokenType.Object => ReadDictionary<Dictionary<string, object>>(token),
                JTokenType.Array => ReadArray(token),
                JTokenType.Integer => token.Value<int>(),
                JTokenType.Float => token.Value<double>(),
                JTokenType.Raw => token.Value<string>(),
                JTokenType.String => token.Value<string>(),
                JTokenType.Uri => token.Value<string>(),
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Date => token.Value<DateTime>(),
                JTokenType.Bytes => token.Value<byte[]>(),
                JTokenType.Guid => token.Value<Guid>(),
                JTokenType.TimeSpan => token.Value<TimeSpan>(),
                JTokenType.Constructor => throw new ArgumentOutOfRangeException(nameof(token.Type), "cannot deserialize a JSON constructor"),
                JTokenType.Property => throw new ArgumentOutOfRangeException(nameof(token.Type), "cannot deserialize a JSON property"),
                JTokenType.Comment => throw new ArgumentOutOfRangeException(nameof(token.Type), "cannot deserialize a JSON comment"),
                _ => throw new ArgumentOutOfRangeException(nameof(token.Type))
            };

        private TDictionary ReadDictionary<TDictionary>(JToken element) where TDictionary : Dictionary<string, object>
        {
            var result = Activator.CreateInstance<TDictionary>();
            foreach (var property in ((JObject)element).Properties())
            {
                if (IsUnsupportedJTokenType(property.Value.Type))
                    continue;
                result[property.Name] = ReadToken(property.Value);
            }
            return result;
        }

        private IEnumerable<object> ReadArray(JToken element)
        {
            foreach (var item in element.Values())
            {
                if (IsUnsupportedJTokenType(item.Type))
                    continue;
                yield return ReadToken(item);
            }
        }

        private bool IsUnsupportedJTokenType(JTokenType type) => type == JTokenType.Constructor || type == JTokenType.Property || type == JTokenType.Comment;
    }
}
