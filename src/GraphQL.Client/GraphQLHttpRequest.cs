using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using GraphQL.Client.Abstractions;

namespace GraphQL.Client.Http
{
    public class GraphQLHttpRequest : GraphQLRequest
    {
        public GraphQLHttpRequest()
        {
        }

        public GraphQLHttpRequest(string query, object? variables = null, string? operationName = null) : base(query, variables, operationName)
        {
        }

        public GraphQLHttpRequest(GraphQLRequest other): base(other.Query, other.Variables, other.OperationName)
        {
        }

        /// <summary>
        /// Allows to preprocess a <see cref="HttpRequestMessage"/> before it is sent, i.e. add custom headers
        /// </summary>
        [IgnoreDataMember]
        [Obsolete("Inherit from GraphQLHttpRequest and override ToHttpRequestMessage() to customize the HttpRequestMessage. Will be removed in v4.0.0.")]
        public Action<HttpRequestMessage> PreprocessHttpRequestMessage { get; set; } = message => { };

        /// <summary>
        /// Creates a <see cref="HttpRequestMessage"/> from this <see cref="GraphQLHttpRequest"/>.
        /// Used by <see cref="GraphQLHttpClient"/> to convert GraphQL requests when sending them as regular HTTP requests.
        /// </summary>
        /// <param name="options">the <see cref="GraphQLHttpClientOptions"/> passed from <see cref="GraphQLHttpClient"/></param>
        /// <param name="serializer">the <see cref="IGraphQLJsonSerializer"/> passed from <see cref="GraphQLHttpClient"/></param>
        /// <returns></returns>
        public virtual HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, options.EndPoint)
            {
                Content = new StringContent(serializer.SerializeToString(this), Encoding.UTF8, options.MediaType)
            };

#pragma warning disable CS0618 // Type or member is obsolete
            PreprocessHttpRequestMessage(message);
#pragma warning restore CS0618 // Type or member is obsolete
            return message;
        }
    }
}
