using GraphQL.Server.Test.GraphQL;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GraphQL.Server.Test {

	public class Startup {

		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
		}

		public void Configure(IApplicationBuilder app) {
			var webHostEnvironment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
			if (webHostEnvironment.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			app.UseHttpsRedirection();

			app.UseWebSockets();
			app.UseGraphQLWebSockets<TestSchema>();
			app.UseGraphQL<TestSchema>();
			app.UseGraphiQLServer(new GraphiQLOptions { });
		}

		public void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<TestSchema>();
			services.AddGraphQL(options => {
				options.EnableMetrics = true;
				options.ExposeExceptions = true;
			}).AddWebSockets();
		}

	}

}
