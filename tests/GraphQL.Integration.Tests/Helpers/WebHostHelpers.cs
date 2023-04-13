using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Http.Websocket;
using GraphQL.Client.Serializer.Newtonsoft;
using IntegrationTestServer;

namespace GraphQL.Integration.Tests.Helpers;

public static class WebHostHelpers
{
    public static async Task<IWebHost> CreateServer(int port)
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection();
        var config = configBuilder.Build();
        config["server.urls"] = $"http://localhost:{port}";

        var host = new WebHostBuilder()
            .ConfigureLogging((ctx, logging) => logging.AddDebug())
            .UseConfiguration(config)
            .UseKestrel()
            .UseStartup<Startup>()
            .Build();

        var tcs = new TaskCompletionSource<bool>();
        host.Services.GetService<IHostApplicationLifetime>().ApplicationStarted.Register(() => tcs.TrySetResult(true));
        await host.StartAsync();
        await tcs.Task;
        return host;
    }

    public static GraphQLHttpClient GetGraphQLClient(
        int port,
        string endpoint,
        IGraphQLWebsocketJsonSerializer? serializer = null,
        Action<GraphQLHttpClientOptions>? configure = null)
    {
        var options = new GraphQLHttpClientOptions();
        configure?.Invoke(options);
        options.EndPoint = new Uri($"http://localhost:{port}{endpoint}");
        return new GraphQLHttpClient(options, serializer ?? new NewtonsoftJsonSerializer());
    }
}
