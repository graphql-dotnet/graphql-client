using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Client {

	public class GraphQLResponse<T> : IEquatable<GraphQLResponse<T>?> {

		public T Data { get; set; }

		public GraphQLError[]? Errors { get; set; }

		public IDictionary<string, dynamic>? Extensions { get; set; }

		public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse<T>);

		public bool Equals(GraphQLResponse<T>? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<dynamic?>.Default.Equals(this.Data, other.Data)) { return false; }
			{
				if (this.Errors != null && other.Errors != null) {
					if (!Enumerable.SequenceEqual(this.Errors, other.Errors)) { return false; }
				}
				else if (this.Errors != null && other.Errors == null) { return false; }
				else if (this.Errors == null && other.Errors != null) { return false; }
			}
			if (!EqualityComparer<IDictionary<string, dynamic>?>.Default.Equals(this.Extensions, other.Extensions)) { return false; }
			return true;
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<dynamic?>.Default.GetHashCode(this.Data);
				{
					if (this.Errors != null) {
						foreach (var element in this.Errors) {
							hashCode = (hashCode * 397) ^ EqualityComparer<GraphQLError?>.Default.GetHashCode(element);
						}
					}
					else {
						hashCode = (hashCode * 397) ^ 0;
					}
				}
				hashCode = (hashCode * 397) ^ EqualityComparer<IDictionary<string, dynamic>?>.Default.GetHashCode(this.Extensions);
				return hashCode;
			}
		}


		public static bool operator ==(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => EqualityComparer<GraphQLResponse<T>?>.Default.Equals(response1, response2);

		public static bool operator !=(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => !(response1 == response2);

	}



}
