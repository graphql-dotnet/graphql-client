using Xunit;

namespace GraphQL.Primitives.Tests;

public class GraphQLLocationTest
{
    [Fact]
    public void ConstructorFact()
    {
        var graphQLLocation = new GraphQLLocation { Column = 1, Line = 2 };
        Assert.Equal(1U, graphQLLocation.Column);
        Assert.Equal(2U, graphQLLocation.Line);
    }

    [Fact]
    public void Equality1Fact()
    {
        var graphQLLocation = new GraphQLLocation { Column = 1, Line = 2 };
        Assert.Equal(graphQLLocation, graphQLLocation);
    }

    [Fact]
    public void Equality2Fact()
    {
        var graphQLLocation1 = new GraphQLLocation { Column = 1, Line = 2 };
        var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
        Assert.Equal(graphQLLocation1, graphQLLocation2);
    }

    [Fact]
    public void EqualityOperatorFact()
    {
        var graphQLLocation1 = new GraphQLLocation { Column = 1, Line = 2 };
        var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
        Assert.True(graphQLLocation1 == graphQLLocation2);
    }

    [Fact]
    public void InEqualityFact()
    {
        var graphQLLocation1 = new GraphQLLocation { Column = 1, Line = 2 };
        var graphQLLocation2 = new GraphQLLocation { Column = 2, Line = 1 };
        Assert.NotEqual(graphQLLocation1, graphQLLocation2);
    }

    [Fact]
    public void InEqualityOperatorFact()
    {
        var graphQLLocation1 = new GraphQLLocation { Column = 1, Line = 2 };
        var graphQLLocation2 = new GraphQLLocation { Column = 2, Line = 1 };
        Assert.True(graphQLLocation1 != graphQLLocation2);
    }

    [Fact]
    public void GetHashCodeFact()
    {
        var graphQLLocation1 = new GraphQLLocation { Column = 1, Line = 2 };
        var graphQLLocation2 = new GraphQLLocation { Column = 1, Line = 2 };
        Assert.True(graphQLLocation1.GetHashCode() == graphQLLocation2.GetHashCode());
    }
}
