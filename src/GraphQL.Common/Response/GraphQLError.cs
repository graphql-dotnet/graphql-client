using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represents the error of a <see cref="GraphQLResponse"/>
	/// </summary>
	public class GraphQLError : IEquatable<GraphQLError> {

		/// <summary>
		/// The error message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The Location of an error
		/// </summary>
		public GraphQLLocation[] Locations { get; set; }

		/// <summary>
		/// Additional error entries
		/// </summary>
		[JsonExtensionData]
		public IDictionary<string, JToken> AdditonalEntries { get; set; }

		/// <inheritdoc />
		public override bool Equals(object obj) => this.Equals(obj as GraphQLError);

		/// <inheritdoc />
		public bool Equals(GraphQLError other) {
			if (other == null) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			if (!Equals(this.Message, other.Message)) {
				return false;
			}
			if (!Equals(this.Locations, other.Locations)) {
				return false;
			}
			if (!Equals(this.AdditonalEntries, other.AdditonalEntries)) {
				return false;
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			var hashCode = 1587536218;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Message);
			hashCode = hashCode * -1521134295 + EqualityComparer<GraphQLLocation[]>.Default.GetHashCode(this.Locations);
			hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, JToken>>.Default.GetHashCode(this.AdditonalEntries);
			return hashCode;
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLError error1, GraphQLError error2) => EqualityComparer<GraphQLError>.Default.Equals(error1, error2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLError error1, GraphQLError error2) => !(error1 == error2);

	}

}
