using System;
using System.Net.Http;

namespace GraphQL.Client.Http {

	public static class HttpClientExtensions {

		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, string endPoint) =>
			new GraphQLHttpClient(endPoint, httpClient);

		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, Uri endPoint) =>
			new GraphQLHttpClient(endPoint, httpClient);

		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, string endPoint, GraphQLHttpClientOptions graphQLHttpClientOptions) =>
			new GraphQLHttpClient(endPoint, graphQLHttpClientOptions, httpClient);

		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, Uri endPoint, GraphQLHttpClientOptions graphQLHttpClientOptions) =>
			new GraphQLHttpClient(endPoint, graphQLHttpClientOptions, httpClient);

	}

}
