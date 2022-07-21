namespace GraphQL;

/// <summary>
/// Represents a GraphQL Location of a GraphQL Query
/// </summary>
public sealed class GraphQLLocation : IEquatable<GraphQLLocation?>
{
    /// <summary>
    /// The Column
    /// </summary>
    public uint Column { get; set; }

    /// <summary>
    /// The Line
    /// </summary>
    public uint Line { get; set; }

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="obj">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLLocation"/> and equals the value of the instance; otherwise, false</returns>
    public override bool Equals(object obj) => Equals(obj as GraphQLLocation);

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="other">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLLocation"/> and equals the value of the instance; otherwise, false</returns>
    public bool Equals(GraphQLLocation? other)
    {
        if (other == null)
        { return false; }
        if (ReferenceEquals(this, other))
        { return true; }
        return EqualityComparer<uint>.Default.Equals(Column, other.Column) &&
            EqualityComparer<uint>.Default.Equals(Line, other.Line);
    }

    /// <summary>
    /// <inheritdoc cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode() =>
        Column.GetHashCode() ^ Line.GetHashCode();

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLLocation"/> instances are equivalent
    /// </summary>
    /// <param name="left">The <see cref="GraphQLLocation"/> instance that is to the left of the equality operator</param>
    /// <param name="right">The <see cref="GraphQLLocation"/> instance that is to the right of the equality operator</param>
    /// <returns>true if left and right are equal; otherwise, false</returns>
    public static bool operator ==(GraphQLLocation? left, GraphQLLocation? right) =>
        EqualityComparer<GraphQLLocation?>.Default.Equals(left, right);

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLLocation"/> instances are not equal
    /// </summary>
    /// <param name="left">The <see cref="GraphQLLocation"/> instance that is to the left of the not equal operator</param>
    /// <param name="right">The <see cref="GraphQLLocation"/> instance that is to the right of the not equal operator</param>
    /// <returns>true if left and right are unequal; otherwise, false</returns>
    public static bool operator !=(GraphQLLocation? left, GraphQLLocation? right) =>
        !EqualityComparer<GraphQLLocation?>.Default.Equals(left, right);
}
