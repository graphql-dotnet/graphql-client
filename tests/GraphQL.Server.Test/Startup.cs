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
			this.LoadPeople().Wait();
		}

		public async Task LoadPeople() {
			using (var httpClient = new HttpClient()) {
				var page = 1;
				var next = true;
				do {
					var body = await httpClient.GetStringAsync($"https://www.swapi.co/api/people/?page={page}");
					var json = JsonConvert.DeserializeObject<JObject>(body);
					var results = json["results"] as JArray;
					foreach (var item in results) {
						var person = item as JObject;
						Storage.People = Storage.People.Append(new Person {
							BirthYear = person["birth_year"].Value<string>(),
							EyeColor = person["eye_color"].Value<string>(),
							Gender = person["gender"].Value<string>(),
							HairColor = person["hair_color"].Value<string>(),
							Height = person["height"].Value<string>(),
							Id = int.Parse(new Uri(person["url"].Value<string>()).Segments[3].Trim('/')),
							Mass = person["mass"].Value<string>(),
							Name = person["name"].Value<string>(),
							SkinColor = person["skin_color"].Value<string>(),
						});
					}
					page++;
					next = (json["next"] as JValue).Value != null;
				} while (next);
			}
		}

	}

}
