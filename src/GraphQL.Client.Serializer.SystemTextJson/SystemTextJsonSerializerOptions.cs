using System.Text.Json;
using Dahomey.Json;

namespace GraphQL.Client.Serializer.SystemTextJson {
	public class SystemTextJsonSerializerOptions {
		public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}.SetupExtensions();
	}
}
