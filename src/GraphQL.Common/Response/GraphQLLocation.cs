using System;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represents the location where the <see cref="GraphQLError"/> has been found
	/// </summary>
	public class GraphQLLocation : IEquatable<GraphQLLocation> {

		#region Properties

		/// <summary>
		/// The Column
		/// </summary>
		public uint Column { get; set; }

		/// <summary>
		/// The Line
		/// </summary>
		public uint Line { get; set; }

		#endregion

		#region IEquatable

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() =>
			this.Column.GetHashCode() ^ this.Line.GetHashCode();

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLLocation"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object obj) {
			if (obj is GraphQLLocation) {
				return Equals(obj as GraphQLLocation);
			}
			return false;
		}

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if other is an instance of <see cref="GraphQLLocation"/> and equals the value of the instance; otherwise, false</returns>
		public bool Equals(GraphQLLocation other) =>
			Equals(this.Column, other.Column) && Equals(this.Line, other.Line);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLLocation"/> instances are equivalent
		/// </summary>
		/// <param name="left">The <see cref="GraphQLLocation"/> instance that is to the left of the equality operator</param>
		/// <param name="right">The <see cref="GraphQLLocation"/> instance that is to the right of the equality operator</param>
		/// <returns>true if left and right are equal; otherwise, false</returns>
		public static bool operator ==(GraphQLLocation left, GraphQLLocation right) =>
			left.Equals(right);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLLocation"/> instances are not equal
		/// </summary>
		/// <param name="left">The <see cref="GraphQLLocation"/> instance that is to the left of the not equal operator</param>
		/// <param name="right">The <see cref="GraphQLLocation"/> instance that is to the right of the not equal operator</param>
		/// <returns>true if left and right are unequal; otherwise, false</returns>
		public static bool operator !=(GraphQLLocation left, GraphQLLocation right) =>
			!left.Equals(right);

		#endregion

	}

}
