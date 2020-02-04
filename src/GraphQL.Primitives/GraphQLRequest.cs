using System;
using System.Collections.Generic;

namespace GraphQL {

	/// <summary>
	/// A GraphQL request
	/// </summary>
	public abstract class GraphQLRequest : IEquatable<GraphQLRequest> {

		/// <summary>
		/// static factory for typed <see cref="GraphQLRequest"/>s
		/// </summary>
		/// <typeparam name="TVariables"></typeparam>
		/// <param name="query"></param>
		/// <param name="variables"></param>
		/// <param name="operationName"></param>
		/// <returns></returns>
		public static GraphQLRequest<TVariables> New<TVariables>(string query, TVariables variables,
			string? operationName = null) {
			return new GraphQLRequest<TVariables>(query, variables, operationName);
		}

		public static GraphQLRequest<object?> New(string query, string? operationName = null) {
			return new GraphQLRequest<object?>(query, null, operationName);
		}

		/// <summary>
		/// The Query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// The name of the Operation
		/// </summary>
		public string? OperationName { get; set; }


		protected GraphQLRequest()
		{
		}

		protected GraphQLRequest(string query, string? operationName = null) {
			Query = query;
			OperationName = operationName;
		}

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="obj">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public override bool Equals(object? obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((GraphQLRequest) obj);
		}

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object
		/// </summary>
		/// <param name="other">The object to compare with this instance</param>
		/// <returns>true if obj is an instance of <see cref="GraphQLRequest"/> and equals the value of the instance; otherwise, false</returns>
		public bool Equals(GraphQLRequest? other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Query == other.Query && OperationName == other.OperationName;
		}

		/// <summary>
		/// <inheritdoc cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				return (Query.GetHashCode() * 397) ^ (OperationName != null ? OperationName.GetHashCode() : 0);
			}
		}

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest{TVariables}"/> instances are equivalent
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest{TVariables}"/> instance that is to the left of the equality operator</param>
		/// <param name="right">The <see cref="GraphQLRequest{TVariables}"/> instance that is to the right of the equality operator</param>
		/// <returns>true if left and right are equal; otherwise, false</returns>
		public static bool operator ==(GraphQLRequest? left, GraphQLRequest? right) => EqualityComparer<GraphQLRequest?>.Default.Equals(left, right);

		/// <summary>
		/// Tests whether two specified <see cref="GraphQLRequest{TVariables}"/> instances are not equal
		/// </summary>
		/// <param name="left">The <see cref="GraphQLRequest{TVariables}"/> instance that is to the left of the not equal operator</param>
		/// <param name="right">The <see cref="GraphQLRequest{TVariables}"/> instance that is to the right of the not equal operator</param>
		/// <returns>true if left and right are unequal; otherwise, false</returns>
		public static bool operator !=(GraphQLRequest? left, GraphQLRequest? right) => !(left == right);


		
	}

	/// <summary>
	/// a GraphQL Request with variables
	/// </summary>
	public class GraphQLRequest<TVariables> : GraphQLRequest, IEquatable<GraphQLRequest<TVariables>?> {

		/// <summary>
		/// Represents the request variables
		/// </summary>
		public TVariables Variables { get; set; }

		public GraphQLRequest()
		{
		}

		public GraphQLRequest(string query, TVariables variables, string? operationName = null) : base(query, operationName) {
			Variables = variables;
		}

		public bool Equals(GraphQLRequest<TVariables>? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return base.Equals(other) && EqualityComparer<TVariables>.Default.Equals(Variables, other.Variables);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((GraphQLRequest<TVariables>) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ EqualityComparer<TVariables>.Default.GetHashCode(Variables);
			}
		}
	}

}
