using System;
using System.Collections.Generic;

namespace GraphQL.Common.Request {

	/// <summary>
	/// A Subscription Request
	/// </summary>
	[Obsolete("EXPERIMENTAL")]
	public class GraphQLWebSocketRequest : IEquatable<GraphQLWebSocketRequest> {

		/// <summary>
		/// The Identifier of the Response
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The Type of the Request
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The Payload of the Request
		/// </summary>
		public GraphQLRequest Payload { get; set; }

		/// <inheritdoc />
		public override bool Equals(object obj) => this.Equals(obj as GraphQLWebSocketRequest);

		/// <inheritdoc />
		public bool Equals(GraphQLWebSocketRequest other) {
			if (other == null) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			if (!Equals(this.Id, other.Id)) {
				return false;
			}
			if (!Equals(this.Type, other.Type)) {
				return false;
			}
			if (!Equals(this.Payload, other.Payload)) {
				return false;
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			var hashCode = 9958074;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Id);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Type);
			hashCode = hashCode * -1521134295 + EqualityComparer<GraphQLRequest>.Default.GetHashCode(this.Payload);
			return hashCode;
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLWebSocketRequest request1, GraphQLWebSocketRequest request2) => EqualityComparer<GraphQLWebSocketRequest>.Default.Equals(request1, request2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLWebSocketRequest request1, GraphQLWebSocketRequest request2) => !(request1 == request2);

	}

}
