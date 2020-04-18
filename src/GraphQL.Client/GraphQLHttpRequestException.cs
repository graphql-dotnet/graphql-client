using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GraphQL.Client.Http
{

    /// <summary>
    /// An exception thrown on unexpected <see cref="System.Net.Http.HttpResponseMessage"/>
    /// </summary>
    public class GraphQLHttpRequestException : Exception
    {
        /// <summary>
        /// The <see cref="System.Net.Http.HttpResponseMessage"/>
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        public HttpResponseHeaders ResponseHeaders { get; }

        public string? Content { get; }

        /// <summary>
        /// Creates a new instance of <see cref="GraphQLHttpRequestException"/>
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="content"></param>
        public GraphQLHttpRequestException(HttpStatusCode statusCode, HttpResponseHeaders responseHeaders, string? content) : base($"The HTTP request failed with status code {statusCode}")
        {
            StatusCode = statusCode;
            ResponseHeaders = responseHeaders;
            Content = content;
        }
    }

}
