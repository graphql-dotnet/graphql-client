using System;
using System.Text.Json;
using System.Threading.Tasks;
using GraphQL.Server.Test.GraphQL.Models;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Http.Examples {

	public class Program {

		private static readonly TestServer testServer = new TestServer(Server.Test.Program.CreateHostBuilder()) {
			AllowSynchronousIO = true
		};

		public static async Task Main(string[] args) {
			using var httpClient = testServer.CreateClient();
			using var graphqlClient = httpClient.AsGraphQLClient($"{testServer.BaseAddress}graphql");
			var graphQLRequest = new GraphQLRequest(
				@"
					{
						repository(owner: ""graphql-dotnet"", name: ""graphql-client"") {
							databaseId,
						    id,
						    name,
						    url
						}
					}"
			);
			var graphQLResponse = await graphqlClient.SendQueryAsync<Schema>(graphQLRequest);
			Console.WriteLine(JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
		}

		private class Schema {

			public Repository Repository { get; set; }

		}

	}

}
