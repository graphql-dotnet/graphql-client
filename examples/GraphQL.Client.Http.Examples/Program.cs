using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Http.Examples {

	public class Program {

		private static readonly TestServer testServer = new TestServer(Server.Test.Program.CreateHostBuilder()) {
			AllowSynchronousIO = true
		};

		public async static Task Main(string[] args) {
			using var httpClient = testServer.CreateClient();
			using var graphqlClient = httpClient.AsGraphQLClient($"{testServer.BaseAddress}graphql");
			var graphQLHttpRequest1 = new GraphQLHttpRequest {

			};
			var graphQLHttpRequest2 = new GraphQLHttpRequest<string> {

			};
			var graphQlHttpResponse1 = await graphqlClient.SendHttpQueryAsync<dynamic>(graphQLHttpRequest1);
			var graphQlHttpResponse2 = await graphqlClient.SendHttpQueryAsync<string, dynamic>(graphQLHttpRequest2);
			Console.WriteLine("Hello World!");
		}

	}

}
