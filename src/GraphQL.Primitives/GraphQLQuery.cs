#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace GraphQL;

/// <summary>
/// Value record for a GraphQL query string
/// </summary>
/// <param name="Text">the actual query string</param>
public readonly record struct GraphQLQuery([StringSyntax("GraphQL")] string Text)
{
    public static implicit operator string(GraphQLQuery query)
        => query.Text;
};
#endif
