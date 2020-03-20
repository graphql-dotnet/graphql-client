using System;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Abstractions
{
    public static class GraphQLClientExtensions
    {
        public static Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(this IGraphQLClient client,
            string query, object? variables = null,
            string? operationName = null, Func<TResponse> defineResponseType = null, CancellationToken cancellationToken = default)
        {
            return client.SendQueryAsync<TResponse>(new GraphQLRequest(query, variables, operationName), cancellationToken: cancellationToken);
        }
        public static Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(this IGraphQLClient client,
            string query, object? variables = null,
            string? operationName = null, Func<TResponse> defineResponseType = null, CancellationToken cancellationToken = default)
        {
            return client.SendMutationAsync<TResponse>(new GraphQLRequest(query, variables, operationName), cancellationToken: cancellationToken);
        }

        public static Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(this IGraphQLClient client,
            GraphQLRequest request, Func<TResponse> defineResponseType, CancellationToken cancellationToken = default)
            => client.SendQueryAsync<TResponse>(request, cancellationToken);

        public static Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(this IGraphQLClient client,
            GraphQLRequest request, Func<TResponse> defineResponseType, CancellationToken cancellationToken = default)
            => client.SendMutationAsync<TResponse>(request, cancellationToken);

        public static IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(
            this IGraphQLClient client, GraphQLRequest request, Func<TResponse> defineResponseType)
            => client.CreateSubscriptionStream<TResponse>(request);

        public static IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(
            this IGraphQLClient client, GraphQLRequest request, Func<TResponse> defineResponseType, Action<Exception> exceptionHandler)
            => client.CreateSubscriptionStream<TResponse>(request, exceptionHandler);
    }
}
