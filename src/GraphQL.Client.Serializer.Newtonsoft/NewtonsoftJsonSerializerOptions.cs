using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client.Serializer.Newtonsoft {
	public class NewtonsoftJsonSerializerOptions {
		public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings {
			ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true }
		};
	}
}
