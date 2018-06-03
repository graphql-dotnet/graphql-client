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
		public string Query { get; set; }

		/// <summary>
		/// If the provided <see cref="Query"/> contains multiple named operations, this specifies which operation should be executed.
		/// </summary>
		public string OperationName { get; set; }

		/// <summary>
		/// The Variables
		/// </summary>
		public dynamic Variables { get; set; }

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
			if (!Equals(this.Query, other.Query)) {
				return false;
			}
			if (!Equals(this.OperationName, other.OperationName)) {
				return false;
			}
			if (!object.Equals(this.Variables, other.Variables)) {
				return false;
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			var hashCode = -689803966;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Query);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.OperationName);
			hashCode = hashCode * -1521134295 + EqualityComparer<dynamic>.Default.GetHashCode(this.Variables);
			return hashCode;
		}

		/// <inheritdoc />
		public static bool operator ==(GraphQLRequest request1, GraphQLRequest request2) {
			if (request1 is null) {
				return request2 is null;
			}

			return request1.Equals(request2);
		}

		/// <inheritdoc />
		public static bool operator !=(GraphQLRequest request1, GraphQLRequest request2) => !(request1 == request2);

	}

}
