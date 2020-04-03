using System;
using System.Net.Http;
using System.Runtime.Serialization;

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

        /// <summary>
        /// Allows to preprocess a <see cref="HttpRequestMessage"/> before it is sent, i.e. add custom headers
        /// </summary>
        [IgnoreDataMember]
        public Action<HttpRequestMessage> PreprocessHttpRequestMessage { get; set; } = message => { };
    }
}
