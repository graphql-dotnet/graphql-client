using System;
using GraphQL.Client;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GraphQL.Integration.Tests.Helpers
{
	public static class WebHostHelpers
	{
		public static IWebHost CreateServer<TStartup>(int port) where TStartup : class
		{
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddInMemoryCollection();
			var config = configBuilder.Build();
			config["server.urls"] = $"http://localhost:{port}";

			var host = new WebHostBuilder()
				.ConfigureLogging((ctx, logging) => {
					logging.AddDebug();
				})
				.UseConfiguration(config)
				.UseKestrel()
				.UseStartup<TStartup>()
				.Build();

			host.Start();

			return host;
		}


		public static GraphQLHttpClient GetGraphQLClient(int port, bool requestsViaWebsocket = false)
			=> new GraphQLHttpClient(new GraphQLHttpClientOptions {
				EndPoint = new Uri($"http://localhost:{port}/graphql"),
				UseWebSocketForQueriesAndMutations = requestsViaWebsocket
			});


		public static TestServerSetup SetupTest<TStartup>(bool requestsViaWebsocket = false) where TStartup : class
		{
			var port = NetworkHelpers.GetFreeTcpPortNumber();
			return new TestServerSetup {
				Server = CreateServer<TStartup>(port),
				Client = GetGraphQLClient(port)
			};
		}
	}

	public class TestServerSetup : IDisposable
	{
		public IWebHost Server { get; set; }
		public IGraphQLClient Client { get; set; }
		public void Dispose()
		{
			Server?.Dispose();
			Client?.Dispose();
		}
	}
}
