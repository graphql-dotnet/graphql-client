using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Server.Test.GraphQL;
using GraphQL.Server.Test.GraphQL.Models;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Server.Test {

	public class Startup {

		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
		}

		public void Configure(IApplicationBuilder app) {
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
