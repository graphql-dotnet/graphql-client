using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.TestHost;
using IntegrationTestServer;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.Helpers
{
    public abstract class TestServerTestFixture : IntegrationServerTestFixture
    {
        private TestServer _testServer;
        public ITestOutputHelper Output { get; set; }
        public override async Task CreateServer()
        {
            var host =
                new WebHostBuilder()
                    .UseStartup<Startup>()
                    .ConfigureLogging((ctx, logging) =>
                    {
                        logging.AddProvider(new XUnitLoggerProvider(Output, new XUnitLoggerOptions()));
                        logging.SetMinimumLevel(LogLevel.Trace);
                    });

            _testServer = new TestServer(host);
            Server = _testServer.Host;
            await _testServer.Host.StartAsync();
        }

        protected override GraphQLHttpClient GetGraphQLClient(string endpoint, bool requestsViaWebsocket = false)
        {
            if (Serializer == null)
                throw new InvalidOperationException("JSON serializer not configured");

            return _testServer.CreateGraphQLHttpClient(new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri($"http://localhost:{Port}{endpoint}"),
                    UseWebSocketForQueriesAndMutations = requestsViaWebsocket
                },
                Serializer);
        }
    }
}
