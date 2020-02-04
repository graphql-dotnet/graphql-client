using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Server.Ui.Playground;
using IntegrationTestServer.ChatSchema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestServer {
	public class Startup {
		public Startup(IConfiguration configuration, IHostingEnvironment environment) {
			Configuration = configuration;
			Environment = environment;
		}

		public IConfiguration Configuration { get; }
		public IHostingEnvironment Environment { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<IChat, Chat>();
			services.AddSingleton<ChatSchema.ChatSchema>();
			services.AddSingleton<ChatQuery>();
			services.AddSingleton<ChatMutation>();
			services.AddSingleton<ChatSubscriptions>();
			services.AddSingleton<MessageType>();
			services.AddSingleton<MessageInputType>();

			services.AddGraphQL(options => {
				options.EnableMetrics = true;
				options.ExposeExceptions = Environment.IsDevelopment();
			})
				.AddWebSockets();

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseWebSockets();
			app.UseGraphQLWebSockets<ChatSchema.ChatSchema>("/graphql");
			app.UseGraphQL<ChatSchema.ChatSchema>("/graphql");
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
			app.UseMvc();
		}
	}
}
