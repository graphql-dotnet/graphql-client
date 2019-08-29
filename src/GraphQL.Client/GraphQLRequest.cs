using System;
using System.Collections.Generic;

namespace GraphQL.Client {

	public class GraphQLRequest<T> : IEquatable<GraphQLRequest<T>?> {

		public string Query { get; set; }
		public string? OperationName { get; set; }
		public T Variables { get; set; }

		public override bool Equals(object? obj) => this.Equals(obj as GraphQLRequest<T>);

		public bool Equals(GraphQLRequest<T>? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<string>.Default.Equals(this.Query, other.Query)) { return false; }
			if (!EqualityComparer<string?>.Default.Equals(this.OperationName, other.OperationName)) { return false; }
			if (!EqualityComparer<dynamic?>.Default.Equals(this.Variables, other.Variables)) { return false; }
			return true;
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<string>.Default.GetHashCode(this.Query);
				hashCode = (hashCode * 397) ^ EqualityComparer<string?>.Default.GetHashCode(this.OperationName);
				hashCode = (hashCode * 397) ^ EqualityComparer<dynamic?>.Default.GetHashCode(this.Variables);
				return hashCode;
			}
		}

		public static bool operator ==(GraphQLRequest<T>? request1, GraphQLRequest<T>? request2) => EqualityComparer<GraphQLRequest<T>?>.Default.Equals(request1, request2);

		public static bool operator !=(GraphQLRequest<T>? request1, GraphQLRequest<T>? request2) => !(request1 == request2);

	}

	public class GraphQLRequest : GraphQLRequest<dynamic?> { }

}
