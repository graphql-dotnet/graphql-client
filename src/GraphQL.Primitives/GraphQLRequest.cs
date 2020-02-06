using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GraphQL {

	/// <summary>
	/// A GraphQL request
	/// </summary>
	public class GraphQLRequest : IEquatable<GraphQLRequest?> {

		/// <summary>
		/// The Query
		/// </summary>
		/// 
		[JsonPropertyName("query")]
		public string Query { get; set; }

		/// <summary>
		/// The name of the Operation
		/// </summary>
		[JsonPropertyName("operationName")]
		public string? OperationName { get; set; }

		/// <summary>
		/// Represents the request variables
		/// </summary>
		[JsonPropertyName("variables")]
		public virtual object? Variables { get; set; }


		public GraphQLRequest() {
		}

		public GraphQLRequest(string query, object? variables = null, string? operationName = null) {
			Query = query;
			Variables = variables;
			OperationName = operationName;
		}

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object? obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((GraphQLRequest)obj);
		}

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public virtual bool Equals(GraphQLRequest? other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Query == other.Query
				   && OperationName == other.OperationName
				   && EqualityComparer<object>.Default.Equals(Variables, other.Variables);
		}

		/// <summary>
		/// <inheritdoc cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				var hashCode = Query.GetHashCode();
				hashCode = (hashCode * 397) ^ OperationName?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ Variables?.GetHashCode() ?? 0;
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


}
