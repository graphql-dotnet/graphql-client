using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Types;

namespace GraphQL.Client.LocalExecution;

public static class GraphQLLocalExecutionClient
{
    public static GraphQLLocalExecutionClient<TSchema> New<TSchema>(TSchema schema, IGraphQLJsonSerializer clientSerializer, IGraphQLTextSerializer serverSerializer)
        where TSchema : ISchema
        => new(schema, new DocumentExecuter(), clientSerializer, serverSerializer);
}

public class GraphQLLocalExecutionClient<TSchema> : IGraphQLClient where TSchema : ISchema
{
    public TSchema Schema { get; }

    public IGraphQLJsonSerializer Serializer { get; }

    private readonly IDocumentExecuter _documentExecuter;
    private readonly IGraphQLTextSerializer _documentSerializer;

    public GraphQLLocalExecutionClient(TSchema schema, IDocumentExecuter documentExecuter, IGraphQLJsonSerializer serializer, IGraphQLTextSerializer documentSerializer)
    {
        Schema = schema ?? throw new ArgumentNullException(nameof(schema), "no schema configured");
        _documentExecuter = documentExecuter;
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer), "please configure the JSON serializer you want to use");
        _documentSerializer = documentSerializer;

        if (!Schema.Initialized)
            Schema.Initialize();
    }

    public void Dispose() { }

    public Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        => ExecuteQueryAsync<TResponse>(request, cancellationToken);

    public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        => ExecuteQueryAsync<TResponse>(request, cancellationToken);

    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request) =>
        Observable.Defer(() => ExecuteSubscriptionAsync<TResponse>(request).ToObservable())
            .Concat()
            .Publish()
            .RefCount();

    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request,
        Action<Exception> exceptionHandler)
        => CreateSubscriptionStream<TResponse>(request);

    #region Private Methods

    private async Task<GraphQLResponse<TResponse>> ExecuteQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken)
    {
        var executionResult = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        return await ExecutionResultToGraphQLResponseAsync<TResponse>(executionResult, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IObservable<GraphQLResponse<TResponse>>> ExecuteSubscriptionAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        var stream = result.Streams?.Values.SingleOrDefault();

        return stream == null
            ? Observable.Throw<GraphQLResponse<TResponse>>(new InvalidOperationException("the GraphQL execution did not return an observable"))
            : stream.SelectMany(executionResult => Observable.FromAsync(token => ExecutionResultToGraphQLResponseAsync<TResponse>(executionResult, token)));
    }

    private async Task<ExecutionResult> ExecuteAsync(GraphQLRequest clientRequest, CancellationToken cancellationToken = default)
    {
        var serverRequest = _documentSerializer.Deserialize<Transport.GraphQLRequest>(Serializer.SerializeToString(clientRequest));

        var result = await _documentExecuter.ExecuteAsync(options =>
        {
            options.Schema = Schema;
            options.OperationName = serverRequest?.OperationName;
            options.Query = serverRequest?.Query;
            options.Variables = serverRequest?.Variables;
            options.Extensions = serverRequest?.Extensions;
            options.CancellationToken = cancellationToken;
        }).ConfigureAwait(false);

        return result;
    }

    private async Task<GraphQLResponse<TResponse>> ExecutionResultToGraphQLResponseAsync<TResponse>(ExecutionResult executionResult, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await _documentSerializer.WriteAsync(stream, executionResult, cancellationToken).ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);
        return await Serializer.DeserializeFromUtf8StreamAsync<TResponse>(stream, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
