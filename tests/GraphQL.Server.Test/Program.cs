using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace GraphQL.Server.Test
{
    public class Program
    {
        public static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync();

        public static IWebHostBuilder CreateHostBuilder(string[] args = null) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.AllowSynchronousIO = true)
                .UseStartup<Startup>();
    }
}
