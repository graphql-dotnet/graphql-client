using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Server.Test {

	public class Startup : IStartup{

		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration){
			this.Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app){
        }

		public IServiceProvider ConfigureServices(IServiceCollection services) {
			return services.BuildServiceProvider();
		}

	}

}
