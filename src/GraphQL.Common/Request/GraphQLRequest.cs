#nullable enable
using System;
using System.Collections.Generic;

namespace GraphQL.Common.Request {

	/// <summary>
	/// Represents a Query that can be fetched to a GraphQL Server.
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#post-request"/>
	/// </summary>
	public class GraphQLRequest : IEquatable<GraphQLRequest?> {

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
		public dynamic? Variables { get; set; }

		/// <summary>
		/// Initialize a new GraphQLRequest
		/// </summary>
		/// <param name="query">The Query</param>
		public GraphQLRequest(string query){
			this.Query = query;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLRequest?);

		/// <inheritdoc />
		public bool Equals(GraphQLRequest? other) {
			if (other == null) {return false;}
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<string>.Default.Equals(this.Query, other.Query)) { return false; }
			if (!EqualityComparer<string?>.Default.Equals(this.OperationName, other.OperationName)) { return false; }
			if (!EqualityComparer<dynamic?>.Default.Equals(this.Variables, other.Variables)) { return false; }
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() => EqualityComparer<GraphQLRequest>.Default.GetHashCode(this);

		/// <inheritdoc />
		public static bool operator ==(GraphQLRequest? request1, GraphQLRequest? request2) => EqualityComparer<GraphQLRequest?>.Default.Equals(request1, request2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLRequest? request1, GraphQLRequest? request2) => !(request1 == request2);

	}

}
