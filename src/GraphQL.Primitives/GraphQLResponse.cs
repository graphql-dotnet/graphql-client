using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GraphQL
{

    public class GraphQLResponse<T> : IEquatable<GraphQLResponse<T>?>
    {

        [DataMember(Name = "data")]
        public T Data { get; set; }

        [DataMember(Name = "errors")]
        public GraphQLError[]? Errors { get; set; }

        [DataMember(Name = "extensions")]
        public GraphQLExtensionsType? Extensions { get; set; }

        public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse<T>);

        public bool Equals(GraphQLResponse<T>? other)
        {
            if (other == null)
            { return false; }
            if (ReferenceEquals(this, other))
            { return true; }
            if (!EqualityComparer<T>.Default.Equals(this.Data, other.Data))
            { return false; }

            if (this.Errors != null && other.Errors != null)
            {
                if (!Enumerable.SequenceEqual(this.Errors, other.Errors))
                { return false; }
            }
            else if (this.Errors != null && other.Errors == null)
            { return false; }
            else if (this.Errors == null && other.Errors != null)
            { return false; }

            if (this.Extensions != null && other.Extensions != null)
            {
                if (!Enumerable.SequenceEqual(this.Extensions, other.Extensions))
                { return false; }
            }
            else if (this.Extensions != null && other.Extensions == null)
            { return false; }
            else if (this.Extensions == null && other.Extensions != null)
            { return false; }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(this.Data);
                {
                    if (this.Errors != null)
                    {
                        foreach (var element in this.Errors)
                        {
                            hashCode = (hashCode * 397) ^ EqualityComparer<GraphQLError?>.Default.GetHashCode(element);
                        }
                    }
                    else
                    {
                        hashCode = (hashCode * 397) ^ 0;
                    }

                    if (this.Extensions != null)
                    {
                        foreach (var element in this.Extensions)
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



}
