using System;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GraphQL.Integration.Tests {
	public static class WebApplicationFactoryExtensions {
		public static GraphQLHttpClient CreateGraphQlHttpClient<TEntryPoint>(
			this WebApplicationFactory<TEntryPoint> factory, string graphQlSchemaUrl, string urlScheme = "http://") where TEntryPoint : class {
			var httpClient = factory.CreateClient();
			var uriBuilder = new UriBuilder(httpClient.BaseAddress);
			uriBuilder.Path = graphQlSchemaUrl;
			uriBuilder.Scheme = urlScheme;
			return httpClient.AsGraphQLClient(uriBuilder.Uri);
		}
	}
}
