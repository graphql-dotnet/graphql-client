using System;
using System.Collections.Generic;

namespace GraphQL {

	/// <summary>
	/// A GraphQL request
	/// </summary>
	public class GraphQLRequest : IEquatable<GraphQLRequest?> {

		/// <summary>
		/// The Query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// The name of the Operation
		/// </summary>
		public string? OperationName { get; set; }

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLRequest);

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public bool Equals(GraphQLRequest? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<string>.Default.Equals(this.Query, other.Query)) { return false; }
			if (!EqualityComparer<string?>.Default.Equals(this.OperationName, other.OperationName)) { return false; }
			return true;
		}

		/// <summary>
		/// <inheritdoc cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<string>.Default.GetHashCode(this.Query);
				hashCode = (hashCode * 397) ^ EqualityComparer<string?>.Default.GetHashCode(this.OperationName);
				return hashCode;
			}
		}

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest"/> instances are equivalent
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the equality operator</param>
		/// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the equality operator</param>
		/// <returns>true if left and right are equal; otherwise, false</returns>
		public static bool operator ==(GraphQLRequest? left, GraphQLRequest? right) => EqualityComparer<GraphQLRequest?>.Default.Equals(left, right);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest"/> instances are not equal
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the not equal operator</param>
		/// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the not equal operator</param>
		/// <returns>true if left and right are unequal; otherwise, false</returns>
		public static bool operator !=(GraphQLRequest? left, GraphQLRequest? right) => !(left == right);

	}

	public class GraphQLRequest<T> : GraphQLRequest, IEquatable<GraphQLRequest<T>?> {

		/// <summary>
		/// Represents the variables sended
		/// </summary>
		public T Variables { get; set; }

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLRequest<T>);

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public bool Equals(GraphQLRequest<T>? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<string>.Default.Equals(this.Query, other.Query)) { return false; }
			if (!EqualityComparer<string?>.Default.Equals(this.OperationName, other.OperationName)) { return false; }
			if (!EqualityComparer<dynamic?>.Default.Equals(this.Variables, other.Variables)) { return false; }
			return true;
		}

		/// <summary>
		/// <inheritdoc cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				return base.GetHashCode() * 397 ^ EqualityComparer<dynamic?>.Default.GetHashCode(this.Variables);
			}
		}

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest"/> instances are equivalent
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the equality operator</param>
		/// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the equality operator</param>
		/// <returns>true if left and right are equal; otherwise, false</returns>
		public static bool operator ==(GraphQLRequest<T>? left, GraphQLRequest<T>? right) => EqualityComparer<GraphQLRequest<T>?>.Default.Equals(left, right);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest"/> instances are not equal
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest"/> instance that is to the left of the not equal operator</param>
		/// <param name="right">The <see cref="GraphQLRequest"/> instance that is to the right of the not equal operator</param>
		/// <returns>true if left and right are unequal; otherwise, false</returns>
		public static bool operator !=(GraphQLRequest<T>? left, GraphQLRequest<T>? right) => !(left == right);

	}

}
