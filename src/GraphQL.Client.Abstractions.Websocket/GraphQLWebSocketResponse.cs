using System;
using System.Collections.Generic;

namespace GraphQL.Client.Abstractions.Websocket
{

    /// <summary>
    /// A Subscription Response
    /// </summary>
    public class GraphQLWebSocketResponse : IEquatable<GraphQLWebSocketResponse>
    {

        /// <summary>
        /// The Identifier of the Response
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Type of the Response
        /// </summary>
        public string Type { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as GraphQLWebSocketResponse);

        /// <inheritdoc />
        public bool Equals(GraphQLWebSocketResponse other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!Equals(this.Id, other.Id))
            {
                return false;
            }

            if (!Equals(this.Type, other.Type))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = 9958074;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Type);
            return hashCode;
        }

        /// <inheritdoc />
        public static bool operator ==(GraphQLWebSocketResponse response1, GraphQLWebSocketResponse response2) =>
            EqualityComparer<GraphQLWebSocketResponse>.Default.Equals(response1, response2);

        /// <inheritdoc />
        public static bool operator !=(GraphQLWebSocketResponse response1, GraphQLWebSocketResponse response2) =>
            !(response1 == response2);

    }

    public class GraphQLWebSocketResponse<TPayload> : GraphQLWebSocketResponse, IEquatable<GraphQLWebSocketResponse<TPayload>>
    {
        public TPayload Payload { get; set; }

        public bool Equals(GraphQLWebSocketResponse<TPayload>? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return base.Equals(other) && Payload.Equals(other.Payload);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((GraphQLWebSocketResponse<TPayload>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Payload.GetHashCode();
            }
        }
    }
}
