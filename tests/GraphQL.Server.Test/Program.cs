using Microsoft.AspNetCore;

namespace GraphQL.Server.Test;

public static class Program
{
    public static async Task Main(string[] args) =>
        await CreateHostBuilder(args).Build().RunAsync();

    public static IWebHostBuilder CreateHostBuilder(string[] args = null) =>
        WebHost.CreateDefaultBuilder(args)
            .UseKestrel(options => options.AllowSynchronousIO = true)
            .UseStartup<Startup>();
}
