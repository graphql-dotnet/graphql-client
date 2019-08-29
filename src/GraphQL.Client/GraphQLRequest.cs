using System;
using System.Collections.Generic;

namespace GraphQL.Client {

	/// <summary>
	/// Represents a Query that can be fetched to a GraphQL Server.
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#post-request"/>
	/// </summary>
	/// <typeparam name="V">The Variable Type</typeparam>
	public class GraphQLRequest<V> : IEquatable<GraphQLRequest<V>?> {

		/// <summary>
		/// The Query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// If the provided <see cref="Query"/> contains multiple named operations, this specifies which operation should be executed.
		/// </summary>
		public string? OperationName { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public V Variables { get; set; }

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLRequest<V>);

		/// <inheritdoc />
		public bool Equals(GraphQLRequest<V>? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<string>.Default.Equals(this.Query, other.Query)) { return false; }
			if (!EqualityComparer<string?>.Default.Equals(this.OperationName, other.OperationName)) { return false; }
			if (!EqualityComparer<dynamic?>.Default.Equals(this.Variables, other.Variables)) { return false; }
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<string>.Default.GetHashCode(this.Query);
				hashCode = (hashCode * 397) ^ EqualityComparer<string?>.Default.GetHashCode(this.OperationName);
				hashCode = (hashCode * 397) ^ EqualityComparer<dynamic?>.Default.GetHashCode(this.Variables);
				return hashCode;
			}
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLRequest<V>? request1, GraphQLRequest<V>? request2) => EqualityComparer<GraphQLRequest<V>?>.Default.Equals(request1, request2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLRequest<V>? request1, GraphQLRequest<V>? request2) => !(request1 == request2);

	}

	public class GraphQLRequest : GraphQLRequest<dynamic?> { }

}
