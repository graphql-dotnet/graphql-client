using GraphQL.Server;
using IntegrationTestServer.ChatSchema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestServer {
	public class StartupChat: Startup {
		public StartupChat(IConfiguration configuration, IWebHostEnvironment environment): base(configuration, environment) { }

		public override void ConfigureGraphQLSchemaServices(IServiceCollection services) {
			services.AddSingleton<IChat, Chat>();
			services.AddSingleton<ChatSchema.ChatSchema>();
			services.AddSingleton<ChatQuery>();
			services.AddSingleton<ChatMutation>();
			services.AddSingleton<ChatSubscriptions>();
			services.AddSingleton<MessageType>();
			services.AddSingleton<MessageInputType>();
		}

		public override void ConfigureGraphQLSchema(IApplicationBuilder app) {
			app.UseGraphQLWebSockets<ChatSchema.ChatSchema>("/graphql");
			app.UseGraphQL<ChatSchema.ChatSchema>("/graphql");
		}
	}
}
