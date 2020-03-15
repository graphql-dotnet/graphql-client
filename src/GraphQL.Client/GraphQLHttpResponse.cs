using System.Net;
using System.Net.Http.Headers;

namespace GraphQL.Client.Http {

	public class GraphQLHttpResponse<T> {
		public GraphQLHttpResponse()
		{
		}
		
		public GraphQLResponse<T> Response { get; set; }
		public HttpResponseHeaders ResponseHeaders { get; set; }
		public HttpStatusCode StatusCode { get; set; }
	}
}
