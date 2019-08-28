using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Client {

	/// <summary>
	/// Represent the response of a <see cref="GraphQLRequest"/>
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#response"/>
	/// </summary>
	/// <typeparam name="T">The Data Type</typeparam>
	public class GraphQLResponse<T> : IEquatable<GraphQLResponse<T>?> {

		/// <summary>
		/// The data of the response
		/// </summary>
		public T Data { get; set; }

		/// <summary>
		/// The Errors if occurred
		/// </summary>
		public GraphQLError[]? Errors { get; set; }

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse<T>);

		/// <inheritdoc />
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
			return true;
		}

		/// <inheritdoc />
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
				return hashCode;
			}
		}


		/// <inheritdoc />
		public static bool operator ==(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => EqualityComparer<GraphQLResponse<T>?>.Default.Equals(response1, response2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLResponse<T>? response1, GraphQLResponse<T>? response2) => !(response1 == response2);

	}

	/// <summary>
	/// The dynamic version of <see cref="GraphQLResponse{T}"/>
	/// </summary>
	public class GraphQLResponse : GraphQLResponse<dynamic?> { }

}
