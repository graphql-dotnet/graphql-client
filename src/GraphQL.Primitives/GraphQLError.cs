using System.Runtime.Serialization;

namespace GraphQL;

/// <summary>
/// Represents a GraphQL Error of a GraphQL Query
/// </summary>
public class GraphQLError : IEquatable<GraphQLError?>
{
    /// <summary>
    /// The locations of the error
    /// </summary>
    [DataMember(Name = "locations")]
    public GraphQLLocation[]? Locations { get; set; }

    /// <summary>
    /// The message of the error
    /// </summary>
    [DataMember(Name = "message")]
    public string Message { get; set; }

    /// <summary>
    /// The Path of the error
    /// </summary>
    [DataMember(Name = "path")]
    public ErrorPath? Path { get; set; }

    /// <summary>
    /// The extensions of the error
    /// </summary>
    [DataMember(Name = "extensions")]
    public Map? Extensions { get; set; }

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="obj">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLError"/> and equals the value of the instance; otherwise, false</returns>
    public override bool Equals(object? obj) => Equals(obj as GraphQLError);

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="other">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLError"/> and equals the value of the instance; otherwise, false</returns>
    public bool Equals(GraphQLError? other)
    {
        if (other == null)
        { return false; }
        if (ReferenceEquals(this, other))
        { return true; }
        {
            if (Locations != null && other.Locations != null)
            {
                if (!Locations.SequenceEqual(other.Locations))
                { return false; }
            }
            else if (Locations != null && other.Locations == null)
            { return false; }
            else if (Locations == null && other.Locations != null)
            { return false; }
        }
        if (!EqualityComparer<string>.Default.Equals(Message, other.Message))
        { return false; }
        {
            if (Path != null && other.Path != null)
            {
                if (!Path.SequenceEqual(other.Path))
                { return false; }
            }
            else if (Path != null && other.Path == null)
            { return false; }
            else if (Path == null && other.Path != null)
            { return false; }
        }
        return true;
    }

    /// <summary>
    /// <inheritdoc cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode()
    {
        var hashCode = 0;
        if (Locations != null)
        {
            hashCode ^= EqualityComparer<GraphQLLocation[]>.Default.GetHashCode(Locations);
        }
        hashCode ^= EqualityComparer<string>.Default.GetHashCode(Message);
        if (Path != null)
        {
            hashCode ^= EqualityComparer<dynamic>.Default.GetHashCode(Path);
        }
        return hashCode;
    }

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLError"/> instances are equivalent
    /// </summary>
    /// <param name="left">The <see cref="GraphQLError"/> instance that is to the left of the equality operator</param>
    /// <param name="right">The <see cref="GraphQLError"/> instance that is to the right of the equality operator</param>
    /// <returns>true if left and right are equal; otherwise, false</returns>
    public static bool operator ==(GraphQLError? left, GraphQLError? right) =>
        EqualityComparer<GraphQLError?>.Default.Equals(left, right);

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLError"/> instances are not equal
    /// </summary>
    /// <param name="left">The <see cref="GraphQLError"/> instance that is to the left of the not equal operator</param>
    /// <param name="right">The <see cref="GraphQLError"/> instance that is to the right of the not equal operator</param>
    /// <returns>true if left and right are unequal; otherwise, false</returns>
    public static bool operator !=(GraphQLError? left, GraphQLError? right) =>
        !EqualityComparer<GraphQLError?>.Default.Equals(left, right);
}
