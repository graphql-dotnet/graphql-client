using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL {

	/// <summary>
	/// <inheritdoc />
	/// </summary>
	public class GraphQLError : IEquatable<GraphQLError?> {

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public IDictionary<string, dynamic>? Extensions { get; set; }

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public GraphQLLocation[]? Locations { get; set; }

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public dynamic[]? Path { get; set; }

		/// <summary>
		///
		/// </summary>
		/// <param name="message"></param>
		public GraphQLError(string message) {
			this.Message = message;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object? obj) =>
			this.Equals(obj as GraphQLError);

		/// <summary>
		///
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(GraphQLError? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<IDictionary<string, dynamic>?>.Default.Equals(this.Extensions, other.Extensions)) { return false; }
			{
				if (this.Locations != null && other.Locations != null) {
					if (!Enumerable.SequenceEqual(this.Locations, other.Locations)) { return false; }
				}
				else if (this.Locations != null && other.Locations == null) { return false; }
				else if (this.Locations == null && other.Locations != null) { return false; }
			}
			if (!EqualityComparer<string>.Default.Equals(this.Message, other.Message)) { return false; }
			{
				if (this.Path != null && other.Path != null) {
					if (!Enumerable.SequenceEqual(this.Path, other.Path)) { return false; }
				}
				else if (this.Path != null && other.Path == null) { return false; }
				else if (this.Path == null && other.Path != null) { return false; }
			}
			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() {
			unchecked {
				var hashCode = EqualityComparer<IDictionary<string, dynamic>?>.Default.GetHashCode(this.Extensions);
				{
					if (this.Locations != null) {
						foreach (var element in this.Locations) {
							hashCode = (hashCode * 397) ^ EqualityComparer<GraphQLLocation?>.Default.GetHashCode(element);
						}
					}
					else {
						hashCode = (hashCode * 397) ^ 0;
					}
				}
				hashCode = (hashCode * 397) ^ EqualityComparer<string>.Default.GetHashCode(this.Message);
				{
					if (this.Path != null) {
						foreach (var element in this.Path) {
							hashCode = (hashCode * 397) ^ EqualityComparer<dynamic?>.Default.GetHashCode(element);
						}
					}
					else {
						hashCode = (hashCode * 397) ^ 0;
					}
				}
				return hashCode;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(GraphQLError? left, GraphQLError? right) =>
			EqualityComparer<GraphQLError?>.Default.Equals(left, right);

		/// <summary>
		///
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(GraphQLError? left, GraphQLError? right) =>
			!EqualityComparer<GraphQLError?>.Default.Equals(left, right);

	}

}
