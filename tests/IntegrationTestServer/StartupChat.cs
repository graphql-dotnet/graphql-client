using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestServer {
	public class StartupChat: Startup {
		public StartupChat(IConfiguration configuration, IWebHostEnvironment environment): base(configuration, environment) { }

		public override void ConfigureGraphQLSchemaServices(IServiceCollection services) {
			services.AddChatSchema();
		}

		public override void ConfigureGraphQLSchema(IApplicationBuilder app) {
			app.UseGraphQLWebSockets<ChatSchema>("/graphql");
			app.UseGraphQL<ChatSchema>("/graphql");
		}
	}
}
