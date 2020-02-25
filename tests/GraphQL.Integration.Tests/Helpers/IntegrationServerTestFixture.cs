using System;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace GraphQL.Integration.Tests.Helpers {
	public abstract class IntegrationServerTestFixture {
		public int Port { get; private set; }
		public IWebHost Server { get; private set; }
		public abstract IGraphQLWebsocketJsonSerializer Serializer { get; }

		public IntegrationServerTestFixture()
		{
			Port = NetworkHelpers.GetFreeTcpPortNumber();
			CreateServer();
		}

		public void CreateServer() {
			if(Server != null)
				throw new InvalidOperationException("server is already created");
			Server = WebHostHelpers.CreateServer(Port);
		}

		public async Task ShutdownServer() {
			if (Server == null)
				return;

			await Server.StopAsync();
			Server.Dispose();
			Server = null;
		}

		public GraphQLHttpClient GetStarWarsClient(bool requestsViaWebsocket = false)
			=> GetGraphQLClient(Common.StarWarsEndpoint, requestsViaWebsocket);

		public GraphQLHttpClient GetChatClient(bool requestsViaWebsocket = false)
			=> GetGraphQLClient(Common.ChatEndpoint, requestsViaWebsocket);

		private GraphQLHttpClient GetGraphQLClient(string endpoint, bool requestsViaWebsocket = false) {
			if(Serializer == null)
				throw new InvalidOperationException("JSON serializer not configured");
			return WebHostHelpers.GetGraphQLClient(Port, endpoint, requestsViaWebsocket, Serializer);
		}
	}

	public class NewtonsoftIntegrationServerTestFixture: IntegrationServerTestFixture {
		public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
	}

	public class SystemTextJsonIntegrationServerTestFixture : IntegrationServerTestFixture {
		public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
	}
}
