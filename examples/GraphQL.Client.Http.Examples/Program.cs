using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Http.Examples {

	public class Program {

		private static readonly TestServer testServer = new TestServer(Server.Test.Program.CreateHostBuilder());

		public async static Task Main(string[] args) {
			using var httpClient = testServer.CreateClient();
			using var graphqlClient = httpClient.AsGraphQLClient();
			Console.WriteLine("Hello World!");
		}

	}

}
