using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Helpers;
using IntegrationTestServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraphQL.Integration.Tests.Helpers
{
    public static class WebHostHelpers
    {
        public static async Task<IWebHost> CreateServer(int port)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection();
            var config = configBuilder.Build();
            config["server.urls"] = $"http://localhost:{port}";

            var host = new WebHostBuilder()
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.AddDebug();
                })
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

        public static GraphQLHttpClient GetGraphQLClient(int port, string endpoint, bool requestsViaWebsocket = false, IGraphQLWebsocketJsonSerializer serializer = null)
            => new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri($"http://localhost:{port}{endpoint}"),
                UseWebSocketForQueriesAndMutations = requestsViaWebsocket,
                JsonSerializer = serializer ?? new NewtonsoftJsonSerializer()
            });
    }

    public class TestServerSetup : IDisposable
    {
        public TestServerSetup(IGraphQLWebsocketJsonSerializer serializer)
        {
            this.serializer = serializer;
            Port = NetworkHelpers.GetFreeTcpPortNumber();
        }

        public int Port { get; }
        public IWebHost Server { get; set; }
        public IGraphQLWebsocketJsonSerializer serializer { get; set; }

        public GraphQLHttpClient GetStarWarsClient(bool requestsViaWebsocket = false)
            => GetGraphQLClient(Common.StarWarsEndpoint, requestsViaWebsocket);

        public GraphQLHttpClient GetChatClient(bool requestsViaWebsocket = false)
            => GetGraphQLClient(Common.ChatEndpoint, requestsViaWebsocket);

        private GraphQLHttpClient GetGraphQLClient(string endpoint, bool requestsViaWebsocket = false)
        {
            return WebHostHelpers.GetGraphQLClient(Port, endpoint, requestsViaWebsocket);
        }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}
