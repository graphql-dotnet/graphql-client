namespace GraphQL;

public interface IGraphQLResponse
{
    object Data { get; }

    GraphQLError[]? Errors { get; set; }

    Map? Extensions { get; set; }
}
