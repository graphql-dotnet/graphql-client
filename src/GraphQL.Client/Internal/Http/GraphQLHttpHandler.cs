using System;
using System.Net.Http;

namespace GraphQL.Client.Internal.Http {

	internal class GraphQLHttpHandler :IDisposable {

		public GraphQLClientOptions Options { get; set; }

		public HttpClient HttpClient { get; set; }

		public GraphQLHttpHandler(GraphQLClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));

			if (this.Options.EndPoint==null) { throw new ArgumentNullException(nameof(this.Options.EndPoint)); }
			if (this.Options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(this.Options.JsonSerializerSettings)); }
			if (this.Options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(this.Options.HttpMessageHandler)); }
			if (this.Options.MediaType == null) { throw new ArgumentNullException(nameof(this.Options.MediaType)); }

			this.HttpClient = new HttpClient(this.Options.HttpMessageHandler);
		}

		public void Dispose() {
			this.HttpClient.Dispose();
			this.Options.HttpMessageHandler.Dispose();
		}

	}

}
