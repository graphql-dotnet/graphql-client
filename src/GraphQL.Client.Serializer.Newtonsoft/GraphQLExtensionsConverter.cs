using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Serializer.Newtonsoft {
	public class GraphQLExtensionsConverter: JsonConverter<GraphQLExtensionsType> {
		public override void WriteJson(JsonWriter writer, GraphQLExtensionsType value, JsonSerializer serializer) {
			throw new NotImplementedException(
				"This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");
		}

		public override GraphQLExtensionsType ReadJson(JsonReader reader, Type objectType, GraphQLExtensionsType existingValue,
			bool hasExistingValue, JsonSerializer serializer) {
			var rootToken = JToken.ReadFrom(reader);
			if (rootToken is JObject) {
				return ReadDictionary<GraphQLExtensionsType>(rootToken);
			}
			else
				throw new ArgumentException("This converter can only parse when the root element is a JSON Object.");
		}

		private object ReadToken(JToken? token) {
			switch (token.Type) {
				case JTokenType.Undefined:
				case JTokenType.None:
					return null;
				case JTokenType.Object:
					return ReadDictionary<Dictionary<string, object>>(token);
				case JTokenType.Array:
					return ReadArray(token);
				case JTokenType.Integer:
					return token.Value<int>();
				case JTokenType.Float:
					return token.Value<double>();
				case JTokenType.Raw:
				case JTokenType.String:
				case JTokenType.Uri:
					return token.Value<string>();
				case JTokenType.Boolean:
					return token.Value<bool>();
				case JTokenType.Date:
					return token.Value<DateTime>();
				case JTokenType.Bytes:
					return token.Value<byte[]>();
				case JTokenType.Guid:
					return token.Value<Guid>();
				case JTokenType.TimeSpan:
					return token.Value<TimeSpan>();
				case JTokenType.Constructor:
				case JTokenType.Property:
				case JTokenType.Comment:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private TDictionary ReadDictionary<TDictionary>(JToken element) where TDictionary : Dictionary<string, object> {
			var result = Activator.CreateInstance<TDictionary>();
			foreach (var property in ((JObject)element).Properties()) {
				result[property.Name] = ReadToken(property.Value);
			}
			return result;
		}

		private IEnumerable<object> ReadArray(JToken element) {
			foreach (var item in element.Values()) {
				yield return ReadToken(item);
			}
		}
	}
}
