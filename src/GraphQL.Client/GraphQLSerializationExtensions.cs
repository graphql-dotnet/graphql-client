using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;
using Newtonsoft.Json;

namespace GraphQL.Client.Http {
	public static class GraphQLSerializationExtensions {

		public static string SerializeToJson(this GraphQLRequest request,
			GraphQLHttpClientOptions options) {
			return JsonConvert.SerializeObject(request, options.JsonSerializerSettings);
		}
		
		public static byte[] SerializeToBytes(this GraphQLRequest request,
			GraphQLHttpClientOptions options) {
			var json = JsonConvert.SerializeObject(request, options.JsonSerializerSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public static TGraphQLResponse DeserializeFromJson<TGraphQLResponse>(this string jsonString,
			GraphQLHttpClientOptions options) {
			return JsonConvert.DeserializeObject<TGraphQLResponse>(jsonString, options.JsonSerializerSettings);
		}

		public static TObject DeserializeFromBytes<TObject>(this byte[] utf8Bytes,
			GraphQLHttpClientOptions options) {
			return JsonConvert.DeserializeObject<TObject>(Encoding.UTF8.GetString(utf8Bytes), options.JsonSerializerSettings);
		}


		public static Task<TGraphQLResponse> DeserializeFromJsonAsync<TGraphQLResponse>(this Stream stream,
			GraphQLHttpClientOptions options, CancellationToken cancellationToken = default) {
			using (StreamReader sr = new StreamReader(stream))
			using (JsonReader reader = new JsonTextReader(sr)) {
				JsonSerializer serializer = JsonSerializer.Create(options.JsonSerializerSettings);
				
				return Task.FromResult(serializer.Deserialize<TGraphQLResponse>(reader));
			}
		}
	}
}
