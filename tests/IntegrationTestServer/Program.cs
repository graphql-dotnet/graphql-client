using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationTestServer {
	public class Program {
		public static void Main(string[] args) {
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<StartupStarWars>()
				.ConfigureLogging((ctx, logging) => logging.SetMinimumLevel(LogLevel.Debug));
	}
}
