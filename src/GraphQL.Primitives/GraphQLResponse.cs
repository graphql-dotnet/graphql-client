using System.Runtime.Serialization;

namespace GraphQL;

public class GraphQLResponse<T> : IGraphQLResponse, IEquatable<GraphQLResponse<T>?>
{
    [DataMember(Name = "data")]
    public T Data { get; set; }
    object IGraphQLResponse.Data => Data;

    [DataMember(Name = "errors")]
    public GraphQLError[]? Errors { get; set; }

    [DataMember(Name = "extensions")]
    public Map? Extensions { get; set; }

    public override bool Equals(object? obj) => Equals(obj as GraphQLResponse<T>);

    public bool Equals(GraphQLResponse<T>? other)
    {
        if (other == null)
        { return false; }
        if (ReferenceEquals(this, other))
        { return true; }
        if (!EqualityComparer<T>.Default.Equals(Data, other.Data))
        { return false; }

        if (Errors != null && other.Errors != null)
        {
            if (!Enumerable.SequenceEqual(Errors, other.Errors))
            { return false; }
        }
        else if (Errors != null && other.Errors == null)
        { return false; }
        else if (Errors == null && other.Errors != null)
        { return false; }

        if (Extensions != null && other.Extensions != null)
        {
            if (!Enumerable.SequenceEqual(Extensions, other.Extensions))
            { return false; }
        }
        else if (Extensions != null && other.Extensions == null)
        { return false; }
        else if (Extensions == null && other.Extensions != null)
        { return false; }

        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = EqualityComparer<T>.Default.GetHashCode(Data);
            {
                if (Errors != null)
                {
                    foreach (var element in Errors)
                    {
                        hashCode = (hashCode * 397) ^ EqualityComparer<GraphQLError?>.Default.GetHashCode(element);
                    }
                }
                else
                {
                    hashCode = (hashCode * 397) ^ 0;
                }

                if (Extensions != null)
                {
                    foreach (var element in Extensions)
                    {
                        hashCode = (hashCode * 397) ^ EqualityComparer<KeyValuePair<string, object>>.Default.GetHashCode(element);
                    }
                }
                else
                {
                    hashCode = (hashCode * 397) ^ 0;
                }
            }
            return hashCode;
        }
    }

    public static bool operator ==(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => EqualityComparer<GraphQLResponse<T>?>.Default.Equals(response1, response2);

    public static bool operator !=(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => !(response1 == response2);
}
