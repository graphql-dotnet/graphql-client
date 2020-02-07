using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL {

	public class GraphQLResponse<T> : IEquatable<GraphQLResponse<T>?> {

		[JsonPropertyName("data")]
		public T Data { get; set; }

		[JsonPropertyName("errors")]
		public GraphQLError[]? Errors { get; set; }

		[JsonPropertyName("extensions")]
		public JsonElement? Extensions { get; set; }

		public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse<T>);

		public bool Equals(GraphQLResponse<T>? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<T>.Default.Equals(this.Data, other.Data)) { return false; }
			{
				if (this.Errors != null && other.Errors != null) {
					if (!Enumerable.SequenceEqual(this.Errors, other.Errors)) { return false; }
				}
				else if (this.Errors != null && other.Errors == null) { return false; }
				else if (this.Errors == null && other.Errors != null) { return false; }
			}
			if (!EqualityComparer<JsonElement?>.Default.Equals(this.Extensions, other.Extensions)) { return false; }
			return true;
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<T>.Default.GetHashCode(this.Data);
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
				hashCode = (hashCode * 397) ^ EqualityComparer<JsonElement?>.Default.GetHashCode(this.Extensions);
				return hashCode;
			}
		}


		public static bool operator ==(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => EqualityComparer<GraphQLResponse<T>?>.Default.Equals(response1, response2);

		public static bool operator !=(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => !(response1 == response2);

	}



}
