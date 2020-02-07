using GraphQL.Server;
using GraphQL.StarWars;
using GraphQL.StarWars.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTestServer {
	public class StartupStarWars: Startup {
		public StartupStarWars(IConfiguration configuration, IWebHostEnvironment environment): base(configuration, environment) { }

		public override void ConfigureGraphQLSchemaServices(IServiceCollection services) {
			services.AddSingleton<StarWarsData>();
			services.AddSingleton<StarWarsQuery>();
			services.AddSingleton<StarWarsMutation>();
			services.AddSingleton<StarWarsSchema>();

			services.AddTransient<CharacterInterface>();
			services.AddTransient<DroidType>();
			services.AddTransient<EpisodeEnum>();
			services.AddTransient<HumanType>();
			services.AddTransient<HumanInputType>();
		}

		public override void ConfigureGraphQLSchema(IApplicationBuilder app) {
			app.UseGraphQLWebSockets<StarWarsSchema>("/graphql");
			app.UseGraphQL<StarWarsSchema>("/graphql");
		}
	}
}
