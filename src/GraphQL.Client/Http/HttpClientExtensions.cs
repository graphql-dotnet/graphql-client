using System;
using System.Net.Http;

namespace GraphQL.Client.Http {

	/// <summary>
	/// Extensions for <see cref="HttpClient"/>
	/// </summary>
	public static class HttpClientExtensions {

		/// <summary>
		/// Creates a <see cref="GraphQLHttpClient"/> from a <see cref="HttpClient"/>
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="graphQLHttpClientOptions"></param>
		/// <returns></returns>
		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, GraphQLHttpClientOptions graphQLHttpClientOptions) =>
			new GraphQLHttpClient(graphQLHttpClientOptions, httpClient);

		/// <summary>
		/// Creates a <see cref="GraphQLHttpClient"/> from a <see cref="HttpClient"/>
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="endPoint"></param>
		/// <returns></returns>
		public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, Uri endPoint) =>
			new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = endPoint }, httpClient);

	}

}
