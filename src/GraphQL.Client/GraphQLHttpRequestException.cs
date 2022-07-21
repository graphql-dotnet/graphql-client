using System.Net;
using System.Net.Http.Headers;

namespace GraphQL.Client.Http;

/// <summary>
/// An exception thrown on unexpected <see cref="System.Net.Http.HttpResponseMessage"/>
/// </summary>
public class GraphQLHttpRequestException : Exception
{
    /// <summary>
    /// The returned status code
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// the returned response headers
    /// </summary>
    public HttpResponseHeaders ResponseHeaders { get; }

    /// <summary>
    /// the returned content
    /// </summary>
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
