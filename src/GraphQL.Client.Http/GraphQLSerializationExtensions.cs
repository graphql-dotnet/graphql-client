using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Http {
	public static class GraphQLSerializationExtensions {

		public static string SerializeToJson(this GraphQLRequest request,
			GraphQLHttpClientOptions options) {
			return JsonSerializer.Serialize(request, options.JsonSerializerOptions);
		}

		public static TGraphQLResponse DeserializeFromJson<TGraphQLResponse>(this string jsonString,
			GraphQLHttpClientOptions options) {
			return JsonSerializer.Deserialize<TGraphQLResponse>(jsonString, options.JsonSerializerOptions);
		}

		public static ValueTask<TGraphQLResponse> DeserializeFromJsonAsync<TGraphQLResponse>(this Stream stream,
			GraphQLHttpClientOptions options, CancellationToken cancellationToken = default) {
			return JsonSerializer.DeserializeAsync<TGraphQLResponse>(stream, options.JsonSerializerOptions, cancellationToken);
		}

	}
}
