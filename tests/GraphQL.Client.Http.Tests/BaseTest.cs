using System;
using Microsoft.AspNetCore.TestHost;

namespace GraphQL.Client.Http.Tests {

	public abstract class BaseTest : IDisposable {

		private readonly TestServer testServer = new TestServer(Server.Test.Program.CreateHostBuilder());

		public void Dispose() {
			this.testServer.Dispose();
		}

	}

}
