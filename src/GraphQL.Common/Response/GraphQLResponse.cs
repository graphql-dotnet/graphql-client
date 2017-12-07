using GraphQL.Common.Request;
using Newtonsoft.Json;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represent the response of a <see cref="GraphQLRequest"/>
	/// Form more information <see href="http://graphql.org/learn/serving-over-http/#response"/>
	/// </summary>
	public class GraphQLResponse {

		public dynamic Data { get; set; }

		public GraphQLError[] Errors { get; set; }

		public dynamic GetData() => JsonConvert.DeserializeObject<dynamic>(this.Data.ToString());

		public Type GetDataFieldAs<Type>(string fieldName) {
			var value = this.Data.GetValue(fieldName);
			return value.ToObject<Type>();
		}

	}

}
