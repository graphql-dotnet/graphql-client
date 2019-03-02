using System;
using System.Linq;
using System.Net.Http;
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

		public Startup(IConfiguration configuration){
			this.Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app){
			{
				var httpClient = new HttpClient();
				{
					var url = "https://swapi.co/api/films/?page=1";
					while (url != null) {
						var jsonObject = JsonConvert.DeserializeObject<JObject>(httpClient.GetStringAsync(url).Result);
						url = jsonObject["next"].Value<string>();
						foreach (var result in jsonObject["results"].As<JArray>()) {
							Storage.Films = Storage.Films.Append(new Film {
								Characters = null,
								Created = result["created"].Value<DateTime>(),
								Director = result["director"].Value<string>(),
								Edited = result["edited"].Value<DateTime>(),
								Id = int.Parse(new Uri(result["url"].Value<string>()).Segments.Last().Trim('/')),
								OpeningCrawl = result["opening_crawl"].Value<string>(),
								Planets = null,
								Producer = result["producer"].Value<string>(),
								ReleaseDate = result["release_date"].Value<string>(),
								Species = null,
								Starships = null,
								Title = result["title"].Value<string>(),
								Vehicles = null,
							});
						}
					}
				}
				{
					var url = "https://swapi.co/api/people/?page=1";
					while (url != null) {
						var jsonObject = JsonConvert.DeserializeObject<JObject>(httpClient.GetStringAsync(url).Result);
						url = jsonObject["next"].Value<string>();
						foreach (var result in jsonObject["results"].As<JArray>()) {
							Storage.Peoples = Storage.Peoples.Append(new People {
								BirthYear = result["birth_year"].Value<string>(),
								Created = result["created"].Value<DateTime>(),
								Edited = result["edited"].Value<DateTime>(),
								EyeColor = result["eye_color"].Value<string>(),
								Films = null,
								Gender = result["gender"].Value<string>(),
								HairColor = result["hair_color"].Value<string>(),
								Height = result["height"].Value<string>(),
								Homeworld = null,
								Id = int.Parse(new Uri(result["url"].Value<string>()).Segments.Last().Trim('/')),
								Mass = result["mass"].Value<string>(),
								Name = result["name"].Value<string>(),
								SkinColor = result["skin_color"].Value<string>(),
								Species = null,
								Starships = null,
								Vehicles = null,
							});
						}
					}
				}
			}
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
