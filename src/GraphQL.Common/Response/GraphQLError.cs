#nullable enable
using System;
using System.Collections.Generic;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represents the error of a <see cref="GraphQLResponse"/>
	/// </summary>
	public class GraphQLError : IEquatable<GraphQLError?> {

		/// <summary>
		/// Additional error entries
		/// </summary>
		public IDictionary<string, dynamic>? Extensions { get; set; }

		/// <summary>
		/// The Location of an error
		/// </summary>
		public GraphQLLocation[]? Locations { get; set; }

		/// <summary>
		/// The error message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The Path of an error
		/// </summary>
		public string[]? Path { get; set; } //TODO it could be also an array of strings and ints at the same time

		/// <summary>
		/// Initialize a new GraphQLError
		/// </summary>
		/// <param name="message">The Message</param>
		public GraphQLError(string message) {
			this.Message = message;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLError);

		/// <inheritdoc />
		public bool Equals(GraphQLError? other) {
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<IDictionary<string, dynamic>?>.Default.Equals(this.Extensions, other.Extensions)) { return false; }
			if (!EqualityComparer<GraphQLLocation[]?>.Default.Equals(this.Locations, other.Locations)) { return false; }
			if (!EqualityComparer<string>.Default.Equals(this.Message, other.Message)) { return false; }
			if (!EqualityComparer<string[]?>.Default.Equals(this.Path, other.Path)) { return false; }
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() => EqualityComparer<GraphQLError>.Default.GetHashCode(this);

		/// <inheritdoc />
		public static bool operator ==(GraphQLError? error1, GraphQLError? error2) => EqualityComparer<GraphQLError?>.Default.Equals(error1, error2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLError? error1, GraphQLError? error2) => !(error1 == error2);

	}

}
