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

		public async static Task Main(string[] args) {
			using var httpClient = testServer.CreateClient();
			using var graphqlClient = httpClient.AsGraphQLClient($"{testServer.BaseAddress}graphql");
			var graphQLHttpRequest = new GraphQLHttpRequest {
				Query = @"
					{
						repository(owner: ""graphql-dotnet"", name: ""graphql-client"") {
							databaseId,
						    id,
						    name,
						    url
						}
					}"
			};
			var graphQlHttpResponse = await graphqlClient.SendHttpQueryAsync<Schema>(graphQLHttpRequest);
			Console.WriteLine(JsonSerializer.Serialize(graphQlHttpResponse, new JsonSerializerOptions{ WriteIndented = true}));
		}

		private class Schema {

			public Repository Repository { get; set; }

		}

	}

}
