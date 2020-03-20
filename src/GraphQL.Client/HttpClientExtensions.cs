using System;
using System.Net.Http;

namespace GraphQL.Client.Http
{

    public static class HttpClientExtensions
    {

        public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, string endPoint) =>
            httpClient.AsGraphQLClient(new Uri(endPoint));

        public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, Uri endPoint) =>
            new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = endPoint }, httpClient);

        public static GraphQLHttpClient AsGraphQLClient(this HttpClient httpClient, GraphQLHttpClientOptions graphQLHttpClientOptions) =>
            new GraphQLHttpClient(graphQLHttpClientOptions, httpClient);
    }

}
