using System;
using System.Collections.Generic;

namespace GraphQL {

	public class GraphQLLocation : IEquatable<GraphQLLocation?> {

		public uint Column { get; set; }

		public uint Line { get; set; }

		public override bool Equals(object obj) {
			var graphQLLocation = obj as GraphQLLocation;
			return graphQLLocation != null &&
				this.Equals(graphQLLocation);
		}

		public bool Equals(GraphQLLocation? other) {
			return other != null &&
				EqualityComparer<uint>.Default.Equals(this.Column, other.Column) &&
				EqualityComparer<uint>.Default.Equals(this.Line, other.Line);
		}

		public override int GetHashCode() =>
			this.Column.GetHashCode() ^ this.Line.GetHashCode();

		public static bool operator ==(GraphQLLocation? left, GraphQLLocation? right) =>
			EqualityComparer<GraphQLLocation?>.Default.Equals(left, right);

		public static bool operator !=(GraphQLLocation? left, GraphQLLocation? right) =>
			!EqualityComparer<GraphQLLocation?>.Default.Equals(left, right);

	}

}
