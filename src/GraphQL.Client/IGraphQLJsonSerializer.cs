using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {
	public interface IGraphQLJsonSerializer {
		string Serialize(GraphQLRequest request);

		Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream,
			CancellationToken cancellationToken);
	}
}
