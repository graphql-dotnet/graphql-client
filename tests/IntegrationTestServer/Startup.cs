using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTestServer {
	public abstract class Startup {
		protected Startup(IConfiguration configuration, IWebHostEnvironment environment) {
			Configuration = configuration;
			Environment = environment;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Environment { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});

			services.AddTransient<IDependencyResolver>(provider => new FuncDependencyResolver(provider.GetService));

			ConfigureGraphQLSchemaServices(services);
			
			services.AddGraphQL(options => {
				options.EnableMetrics = true;
				options.ExposeExceptions = Environment.IsDevelopment();
			})
				.AddWebSockets();
		}

		public abstract void ConfigureGraphQLSchemaServices(IServiceCollection services);


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseWebSockets();

			ConfigureGraphQLSchema(app);

			app.UseGraphiQLServer(new GraphiQLOptions {
				GraphiQLPath = "/ui/graphiql",
				GraphQLEndPoint = "/graphql"
			});
			app.UseGraphQLVoyager(new GraphQLVoyagerOptions() {
				GraphQLEndPoint = "/graphql",
				Path = "/ui/voyager"
			});
			app.UseGraphQLPlayground(new GraphQLPlaygroundOptions {
				Path = "/ui/playground"
			});
		}

		public abstract void ConfigureGraphQLSchema(IApplicationBuilder app);
	}
}
