using System;
using System.Collections.Generic;

namespace GraphQL.Common.Request {

	/// <summary>
	/// Represents a Query that can be fetched to a GraphQL Server.
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#post-request"/>
	/// </summary>
	public class GraphQLRequest : IEquatable<GraphQLRequest> {

		/// <summary>
		/// The Query
		/// </summary>
		public string query { get; set; }

		/// <summary>
		/// If the provided <see cref="query"/> contains multiple named operations, this specifies which operation should be executed.
		/// </summary>
		public string operationName { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public dynamic variables { get; set; }

		/// <inheritdoc />
		public override bool Equals(object obj) => this.Equals(obj as GraphQLRequest);

		/// <inheritdoc />
		public bool Equals(GraphQLRequest other) {
			if (other == null) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			if (!Equals(this.query, other.query)) {
				return false;
			}
			if (!Equals(this.operationName, other.operationName)) {
				return false;
			}
			if (!Equals(this.variables, other.variables)) {
				return false;
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			var hashCode = -689803966;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.query);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.operationName);
			hashCode = hashCode * -1521134295 + EqualityComparer<dynamic>.Default.GetHashCode(this.variables);
			return hashCode;
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLRequest request1, GraphQLRequest request2) => EqualityComparer<GraphQLRequest>.Default.Equals(request1, request2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLRequest request1, GraphQLRequest request2) => !(request1 == request2);

	}

}
