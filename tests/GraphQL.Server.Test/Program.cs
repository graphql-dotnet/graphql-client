using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GraphQL.Server.Test {

	public class Program {

		// This exposes https://swapi.co/ via GraphQL with some additions
		public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

		public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>{
                    webBuilder.UseStartup<Startup>();
					webBuilder.UseKestrel(options => { options.AllowSynchronousIO = true; });
				});

    }

}
