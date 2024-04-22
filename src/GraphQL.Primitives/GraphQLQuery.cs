#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace GraphQL;

public readonly record struct GraphQLQuery([StringSyntax("GraphQL")] string Text)
{
    public static implicit operator string(GraphQLQuery query)
        => query.Text;
};
#endif
