#if !NET7_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Stub
/// </summary>
public sealed class StringSyntaxAttribute : Attribute
{
    public const string CompositeFormat = nameof(CompositeFormat);

    public StringSyntaxAttribute(string syntax)
    {
    }

}
#endif
