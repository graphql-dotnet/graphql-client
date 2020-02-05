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
	}
}
