using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represent the response of a <see cref="GraphQLQuery"/>
	/// </summary>
	public class GraphQLResponse {

		public JObject Data { get; set; }

		public GraphQLError[] Errors { get; set; }

		public dynamic GetData() => JsonConvert.DeserializeObject<dynamic>(this.Data.ToString());

		public Type GetDataFieldAs<Type>(string fieldName) {
			var value = this.Data.GetValue(fieldName);
			return value.ToObject<Type>();
		}

	}

}
