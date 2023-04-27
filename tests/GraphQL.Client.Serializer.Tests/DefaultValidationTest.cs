using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using GraphQL.Client.Http;

using Xunit;

namespace GraphQL.Client.Serializer.Tests;

public class DefaultValidationTest
{

    [Theory]
    [InlineData(HttpStatusCode.OK, "application/json", true)]
    [InlineData(HttpStatusCode.OK, "application/graphql-response+json", true)]
    [InlineData(HttpStatusCode.BadRequest, "application/json", true)]
    [InlineData(HttpStatusCode.BadRequest, "text/html", false)]
    [InlineData(HttpStatusCode.OK, "text/html", false)]
    [InlineData(HttpStatusCode.Forbidden, "text/html", false)]
    [InlineData(HttpStatusCode.Forbidden, "application/json", false)]
    public void IsValidResponse_OkJson_True(HttpStatusCode statusCode, string mediaType, bool expectedResult)
    {
        var response = new HttpResponseMessage(statusCode);
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

        bool result = new GraphQLHttpClientOptions().IsValidResponseToDeserialize(response);

        result.Should().Be(expectedResult);
    }
}
