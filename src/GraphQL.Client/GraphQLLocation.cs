using System;
using System.Collections.Generic;

namespace GraphQL.Client {

	public class GraphQLLocation : IEquatable<GraphQLLocation?> {

		public uint Column { get; set; }

		public uint Line { get; set; }

		public override bool Equals(object? obj) => this.Equals(obj as GraphQLLocation);

		public bool Equals(GraphQLLocation? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<uint>.Default.Equals(this.Column, other.Column)) { return false; }
			if (!EqualityComparer<uint>.Default.Equals(this.Line, other.Line)) { return false; }
			return true;
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<uint>.Default.GetHashCode(this.Column);
				hashCode = (hashCode * 397) ^ EqualityComparer<uint>.Default.GetHashCode(this.Line);
				return hashCode;
			}
		}

		public static bool operator ==(GraphQLLocation? location1, GraphQLLocation? location2) => EqualityComparer<GraphQLLocation?>.Default.Equals(location1, location2);

		public static bool operator !=(GraphQLLocation? location1, GraphQLLocation? location2) => !(location1 == location2);

	}

}
