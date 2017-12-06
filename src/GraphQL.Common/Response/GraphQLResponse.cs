using Newtonsoft.Json.Linq;

namespace GraphQL.Client.Response {

	public class GraphQLResponse {

		public JObject Data { get; set; }

		public GraphQLError[] Errors { get; set; }

		public Type GetDataFieldAs<Type>(string fieldName) {
			var value = this.Data.GetValue(fieldName);
			return value.ToObject<Type>();
		}

	}

}
