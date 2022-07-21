namespace GraphQL.Client.Abstractions;

public interface IGraphQLJsonSerializer
{
    string SerializeToString(GraphQLRequest request);

    Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken);
}
