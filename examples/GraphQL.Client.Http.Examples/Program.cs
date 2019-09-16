using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Http.Examples {

	public class Program {

		private static readonly TestServer testServer = new TestServer(Server.Test.Program.CreateHostBuilder());

		public async static Task Main(string[] args) {
			var httpClient = testServer.CreateWebSocketClient();
			Console.WriteLine("Hello World!");
		}

	}

}
