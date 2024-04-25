using System.Diagnostics.CodeAnalysis;
namespace GraphQL;

/// <summary>
/// Value object representing a GraphQL query string and storing the corresponding APQ hash. <br />
/// Use this to hold query strings you want to use more than once.
/// </summary>
public class GraphQLQuery : IEquatable<GraphQLQuery>
{
    /// <summary>
    /// The actual query string
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The SHA256 hash used for the advanced persisted queries feature (APQ)
    /// </summary>
    public string Sha256Hash { get; }

    public GraphQLQuery([StringSyntax("GraphQL")] string text)
    {
        Text = text;
        Sha256Hash = Hash.Compute(Text);
    }

    public static implicit operator string(GraphQLQuery query)
        => query.Text;

    public bool Equals(GraphQLQuery other) => Sha256Hash == other.Sha256Hash;

    public override bool Equals(object? obj) => obj is GraphQLQuery other && Equals(other);

    public override int GetHashCode() => Sha256Hash.GetHashCode();
}
