using System;
using System.Collections.Generic;

namespace GraphQL.Common.Response {

	/// <summary>
	/// A Subscription Response
	/// </summary>
	[Obsolete("EXPERIMENTAL")]
	public class GraphQLWebSocketResponse : IEquatable<GraphQLWebSocketResponse> {

		/// <summary>
		/// The Identifier of the Response
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The Type of the Response
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The Payload of the Response
		/// </summary>
		public dynamic Payload { get; set; }

		/// <inheritdoc />
		public override bool Equals(object obj) => this.Equals(obj as GraphQLWebSocketResponse);

		/// <inheritdoc />
		public bool Equals(GraphQLWebSocketResponse other) {
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
			hashCode = hashCode * -1521134295 + EqualityComparer<GraphQLResponse>.Default.GetHashCode(this.Payload);
			return hashCode;
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLWebSocketResponse response1, GraphQLWebSocketResponse response2) => EqualityComparer<GraphQLWebSocketResponse>.Default.Equals(response1, response2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLWebSocketResponse response1, GraphQLWebSocketResponse response2) => !(response1 == response2);

	}

}
