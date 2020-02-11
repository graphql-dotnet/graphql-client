using System.Text.Json;
using Dahomey.Json;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class SystemTextJsonSerializerOptions {
		public JsonSerializerOptions JsonSerializerOptions { get; set; } = DefaultJsonSerializerOptions;

		public static JsonSerializerOptions DefaultJsonSerializerOptions {
			get {
				var options = new JsonSerializerOptions {
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				};

				options.Converters.Add(new GraphQLExtensionsConverter());

				// Use extensions brought by Dahomey.Json to allow to deserialize anonymous types
				options.SetupExtensions(); 

				return options;
			}
		}
	}
}
