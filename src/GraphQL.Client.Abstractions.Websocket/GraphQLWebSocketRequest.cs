using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.Client.Abstractions.Websocket
{

    public class GraphQLInitAuthWebsocketRequest : GraphQLWebSocketRequest
    {
        public GraphQLInitAuthWebsocketRequest(string id, string token)
        {
            Id = id;
            Type = GraphQLWebSocketMessageType.GQL_CONNECTION_INIT;
            this[PAYLOAD_KEY] = new Dictionary<string, string>() { { "Authorization", token } };
        }
    }

    /// <summary>
    /// A Subscription Request
    /// </summary>
    public class GraphQLWebSocketRequest : Dictionary<string, object>, IEquatable<GraphQLWebSocketRequest>
    {
        public const string ID_KEY = "id";
        public const string TYPE_KEY = "type";
        public const string PAYLOAD_KEY = "payload";

        /// <summary>
        /// The Identifier of the Response
        /// </summary>
        public string Id
        {
            get => ContainsKey(ID_KEY) ? (string)this[ID_KEY] : null;
            set => this[ID_KEY] = value;
        }

        /// <summary>
        /// The Type of the Request
        /// </summary>
        public string Type
        {
            get => ContainsKey(TYPE_KEY) ? (string)this[TYPE_KEY] : null;
            set => this[TYPE_KEY] = value;
        }

        /// <summary>
        /// The payload of the websocket request
        /// </summary>
        public GraphQLRequest Payload
        {
            get => ContainsKey(PAYLOAD_KEY) ? (GraphQLRequest)this[PAYLOAD_KEY] : null;
            set => this[PAYLOAD_KEY] = value;
        }

        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        /// <summary>
        /// Task used to await the actual send operation and to convey potential exceptions
        /// </summary>
        /// <returns></returns>
        public Task SendTask() => _tcs.Task;

        /// <summary>
        /// gets called when the send operation for this request has completed successfully
        /// </summary>
        public void SendCompleted() => _tcs.SetResult(true);

        /// <summary>
        /// gets called when an exception occurs during the send operation
        /// </summary>
        /// <param name="e"></param>
        public void SendFailed(Exception e) => _tcs.SetException(e);

        /// <summary>
        /// gets called when the GraphQLHttpWebSocket has been disposed before the send operation for this request has started
        /// </summary>
        public void SendCanceled() => _tcs.SetCanceled();

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as GraphQLWebSocketRequest);

        /// <inheritdoc />
        public bool Equals(GraphQLWebSocketRequest other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (!Equals(Id, other.Id))
            {
                return false;
            }
            if (!Equals(Type, other.Type))
            {
                return false;
            }
            if (!Equals(Payload, other.Payload))
            {
                return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = 9958074;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Payload);
            return hashCode;
        }

        /// <inheritdoc />
        public static bool operator ==(GraphQLWebSocketRequest request1, GraphQLWebSocketRequest request2) => EqualityComparer<GraphQLWebSocketRequest>.Default.Equals(request1, request2);

        /// <inheritdoc />
        public static bool operator !=(GraphQLWebSocketRequest request1, GraphQLWebSocketRequest request2) => !(request1 == request2);
    }
}
