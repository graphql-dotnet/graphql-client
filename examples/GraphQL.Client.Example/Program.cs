using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Client.Http.Examples {

	public class Program {

		public static async Task Main(string[] args) {

			using var graphQLClient = new GraphQLHttpClient("https://swapi.apis.guru/");

			var personAndFilmsRequest = new GraphQLRequest {
				Query = @"
			    query PersonAndFilms($id: ID) {
			        person(id: $id) {
			            name
			            filmConnection {
			                films {
			                    title
			                }
			            }
			        }
			    }",
				OperationName = "PersonAndFilms",
				Variables = new {
					id = "cGVvcGxlOjE="
				}
			};

			var graphQLResponse = await graphQLClient.SendQueryAsync<PersonAndFilmsResponse>(personAndFilmsRequest);
			Console.WriteLine("raw response:");
			Console.WriteLine(JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));

			Console.WriteLine();
			Console.WriteLine($"Name: {graphQLResponse.Data.Person.Name}" );
			var films = string.Join(", ", graphQLResponse.Data.Person.FilmConnection.Films.Select(f => f.Title));
			Console.WriteLine($"Films: {films}");

			Console.WriteLine();
			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
		}

	}
}
