using GraphQL.Client.Abstractions;
using GraphQL.DI;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Client.LocalExecution;

public static class ServiceCollectionExtensions
{
    public static IGraphQLBuilder AddGraphQLLocalExecutionClient<TSchema>(this IServiceCollection services) where TSchema : ISchema
    {
        services.AddSingleton<GraphQLLocalExecutionClient<TSchema>>();
        services.AddSingleton<IGraphQLClient>(p => p.GetRequiredService<GraphQLLocalExecutionClient<TSchema>>());
        return new GraphQLBuilder(services, null);
    }
}
