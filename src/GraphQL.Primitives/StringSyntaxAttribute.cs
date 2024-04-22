#if !NET7_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Stub
/// </summary>
public sealed class StringSyntaxAttribute : Attribute
{
    // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006
    public const string CompositeFormat = nameof(CompositeFormat);
#pragma warning restore IDE1006

#pragma warning disable IDE0060
    public StringSyntaxAttribute(string syntax)
    { }
#pragma warning restore IDE0060
}
#endif
