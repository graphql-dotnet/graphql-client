using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json;
using Dahomey.Json.Serialization.Converters.Factories;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public static class JsonSerializerOptionsExtensions {
		public static JsonSerializerOptions SetupDahomeyExtensions(
			this JsonSerializerOptions options) {
			options.Converters.Add(new ImmutableConverter());
			//options.Converters.Add((JsonConverter)new JsonSerializerOptionsState(options));
			//options.Converters.Add((JsonConverter)new DictionaryConverterFactory());
			//options.Converters.Add((JsonConverter)new CollectionConverterFactory());
			//options.Converters.Add((JsonConverter)new JsonNodeConverterFactory());
			//options.Converters.Add((JsonConverter)new ObjectConverterFactory());
			return options;
		}
	}
}
