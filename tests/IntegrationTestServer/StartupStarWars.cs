using GraphQL.Client.Tests.Common;
using GraphQL.Server;
using GraphQL.StarWars;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestServer {
	public class StartupStarWars: Startup {
		public StartupStarWars(IConfiguration configuration, IWebHostEnvironment environment): base(configuration, environment) { }

		public override void ConfigureGraphQLSchemaServices(IServiceCollection services) {
			services.AddStarWarsSchema();
		}

		public override void ConfigureGraphQLSchema(IApplicationBuilder app) {
			app.UseGraphQLWebSockets<StarWarsSchema>("/graphql");
			app.UseGraphQL<StarWarsSchema>("/graphql");
		}
	}
}
