namespace GraphQL;

/// <summary>
/// A GraphQL request
/// </summary>
public class GraphQLRequest : Dictionary<string, object>, IEquatable<GraphQLRequest?>
{
    public const string OPERATION_NAME_KEY = "operationName";
    public const string QUERY_KEY = "query";
    public const string VARIABLES_KEY = "variables";
    public const string EXTENSIONS_KEY = "extensions";

    /// <summary>
    /// The Query
    /// </summary>
    public string Query
    {
        get => TryGetValue(QUERY_KEY, out object value) ? (string)value : null;
        set => this[QUERY_KEY] = value;
    }

    /// <summary>
    /// The name of the Operation
    /// </summary>
    public string? OperationName
    {
        get => TryGetValue(OPERATION_NAME_KEY, out object value) ? (string)value : null;
        set => this[OPERATION_NAME_KEY] = value;
    }

    /// <summary>
    /// Represents the request variables
    /// </summary>
    public object? Variables
    {
        get => TryGetValue(VARIABLES_KEY, out object value) ? value : null;
        set => this[VARIABLES_KEY] = value;
    }

    /// <summary>
    /// Represents the request extensions
    /// </summary>
    public object? Extensions
    {
        get => TryGetValue(EXTENSIONS_KEY, out object value) ? value : null;
        set => this[EXTENSIONS_KEY] = value;
    }

    public GraphQLRequest() { }

    public GraphQLRequest(string query, object? variables = null, string? operationName = null, object? extensions = null)
    {
        Query = query;
        Variables = variables;
        OperationName = operationName;
        Extensions = extensions;
    }

    public GraphQLRequest(GraphQLRequest other) : base(other) { }

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="obj">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((GraphQLRequest)obj);
    }

    /// <summary>
    /// Returns a value that indicates whether this instance is equal to a specified object
    /// </summary>
    /// <param name="other">The object to compare with this instance</param>
    /// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
    public virtual bool Equals(GraphQLRequest? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Count == other.Count && !this.Except(other).Any();
    }

    /// <summary>
    /// <inheritdoc cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode() => (Query, OperationName, Variables, Extensions).GetHashCode();

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLRequest"/> instances are equivalent
    /// </summary>
    /// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the equality operator</param>
    /// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the equality operator</param>
    /// <returns>true if left and right are equal; otherwise, false</returns>
    public static bool operator ==(GraphQLRequest? left, GraphQLRequest? right) => EqualityComparer<GraphQLRequest?>.Default.Equals(left, right);

    /// <summary>
    /// Tests whether two specified <see cref="GraphQLRequest"/> instances are not equal
    /// </summary>
    /// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the not equal operator</param>
    /// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the not equal operator</param>
    /// <returns>true if left and right are unequal; otherwise, false</returns>
    public static bool operator !=(GraphQLRequest? left, GraphQLRequest? right) => !(left == right);
}
