#nullable enable
using System;
using System.Collections.Generic;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represents the location where the <see cref="GraphQLError"/> has been found
	/// </summary>
	public class GraphQLLocation : IEquatable<GraphQLLocation?> {

		/// <summary>
		/// The Column
		/// </summary>
		public uint Column { get; set; }

		/// <summary>
		/// The Line
		/// </summary>
		public uint Line { get; set; }

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLLocation?);

		/// <inheritdoc />
		public bool Equals(GraphQLLocation? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<uint>.Default.Equals(this.Column, other.Column)) { return false; }
			if (!EqualityComparer<uint>.Default.Equals(this.Line, other.Line)) { return false; }
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() => EqualityComparer<GraphQLLocation?>.Default.GetHashCode(this);

		/// <inheritdoc />
		public static bool operator ==(GraphQLLocation? location1, GraphQLLocation? location2) => EqualityComparer<GraphQLLocation?>.Default.Equals(location1, location2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLLocation? location1, GraphQLLocation? location2) => !(location1 == location2);

	}

}
