using Microsoft.AspNetCore;

namespace IntegrationTestServer;

public static class Program
{
    public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((_, logging) => logging.SetMinimumLevel(LogLevel.Debug));
}
