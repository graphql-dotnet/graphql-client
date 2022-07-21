using Xunit;

namespace GraphQL.Primitives.Tests;

public class GraphQLResponseTest
{
    [Fact]
    public void Constructor1Fact()
    {
        var graphQLResponse = new GraphQLResponse<object>();
        Assert.Null(graphQLResponse.Data);
        Assert.Null(graphQLResponse.Errors);
    }

    [Fact]
    public void Constructor2Fact()
    {
        var graphQLResponse = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        Assert.NotNull(graphQLResponse.Data);
        Assert.NotNull(graphQLResponse.Errors);
    }

    [Fact]
    public void Equality1Fact()
    {
        var graphQLResponse = new GraphQLResponse<object>();
        Assert.Equal(graphQLResponse, graphQLResponse);
    }

    [Fact]
    public void Equality2Fact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>();
        var graphQLResponse2 = new GraphQLResponse<object>();
        Assert.Equal(graphQLResponse1, graphQLResponse2);
    }

    [Fact]
    public void Equality3Fact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        var graphQLResponse2 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        Assert.Equal(graphQLResponse1, graphQLResponse2);
    }

    [Fact]
    public void EqualityOperatorFact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>();
        var graphQLResponse2 = new GraphQLResponse<object>();
        Assert.True(graphQLResponse1 == graphQLResponse2);
    }

    [Fact]
    public void InEqualityFact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        var graphQLResponse2 = new GraphQLResponse<object>
        {
            Data = new { a = 2 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        Assert.NotEqual(graphQLResponse1, graphQLResponse2);
    }

    [Fact]
    public void InEqualityOperatorFact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        var graphQLResponse2 = new GraphQLResponse<object>
        {
            Data = new { a = 2 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        Assert.True(graphQLResponse1 != graphQLResponse2);
    }

    [Fact]
    public void GetHashCodeFact()
    {
        var graphQLResponse1 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        var graphQLResponse2 = new GraphQLResponse<object>
        {
            Data = new { a = 1 },
            Errors = new[] { new GraphQLError { Message = "message" } }
        };
        Assert.True(graphQLResponse1.GetHashCode() == graphQLResponse2.GetHashCode());
    }
}
