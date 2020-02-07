using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GraphQL {

	/// <summary>
	/// Represents a GraphQL Error of a GraphQL Query
	/// </summary>
	public class GraphQLError : IEquatable<GraphQLError?> {

		/// <summary>
		/// The extensions of the error
		/// </summary> 
		[DataMember(Name = "extensions")]
		public IDictionary<string, object?>? Extensions { get; set; }

		/// <summary>
		/// The locations of the error
		/// </summary>
		[DataMember(Name = "locations")]
		public GraphQLLocation[]? Locations { get; set; }

		/// <summary>
		/// The message of the error
		/// </summary>
		[DataMember(Name = "message")]
		public string Message { get; set; }

		/// <summary>
		/// The Path of the error
		/// </summary>
		[DataMember(Name = "path")]
		public object[]? Path { get; set; }

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLError"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object? obj) =>
			this.Equals(obj as GraphQLError);

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLError"/> and equals the value of the instance; otherwise, false</returns>
		public bool Equals(GraphQLError? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<IDictionary<string, object?>?>.Default.Equals(this.Extensions, other.Extensions)) { return false; }
			{
				if (this.Locations != null && other.Locations != null) {
					if (!this.Locations.SequenceEqual(other.Locations)) { return false; }
				}
				else if (this.Locations != null && other.Locations == null) { return false; }
				else if (this.Locations == null && other.Locations != null) { return false; }
			}
			if (!EqualityComparer<string>.Default.Equals(this.Message, other.Message)) { return false; }
			{
				if (this.Path != null && other.Path != null) {
					if (!this.Path.SequenceEqual(other.Path)) { return false; }
				}
				else if (this.Path != null && other.Path == null) { return false; }
				else if (this.Path == null && other.Path != null) { return false; }
			}
			return true;
		}

		/// <summary>
		/// <inheritdoc cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode() {
			var hashCode = 0;
			if (this.Extensions != null) {
				hashCode = hashCode ^ EqualityComparer<IDictionary<string, object?>?>.Default.GetHashCode(this.Extensions);
			}
			if (this.Locations != null) {
				hashCode = hashCode ^ EqualityComparer<GraphQLLocation[]>.Default.GetHashCode(this.Locations);
			}
			hashCode = hashCode ^ EqualityComparer<string>.Default.GetHashCode(this.Message);
			if (this.Path != null) {
				hashCode = hashCode ^ EqualityComparer<dynamic>.Default.GetHashCode(this.Path);
			}
			return hashCode;
		}

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLError"/> instances are equivalent
		/// </summary>
		/// <param name="left">The <see cref="GraphQLError"/> instance that is to the left of the equality operator</param>
		/// <param name="right">The <see cref="GraphQLError"/> instance that is to the right of the equality operator</param>
		/// <returns>true if left and right are equal; otherwise, false</returns>
		public static bool operator ==(GraphQLError? left, GraphQLError? right) =>
			EqualityComparer<GraphQLError?>.Default.Equals(left, right);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLError"/> instances are not equal
		/// </summary>
		/// <param name="left">The <see cref="GraphQLError"/> instance that is to the left of the not equal operator</param>
		/// <param name="right">The <see cref="GraphQLError"/> instance that is to the right of the not equal operator</param>
		/// <returns>true if left and right are unequal; otherwise, false</returns>
		public static bool operator !=(GraphQLError? left, GraphQLError? right) =>
			!EqualityComparer<GraphQLError?>.Default.Equals(left, right);

	}

}
