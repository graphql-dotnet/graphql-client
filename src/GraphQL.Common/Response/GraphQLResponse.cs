#nullable enable
using System;
using System.Collections.Generic;
using GraphQL.Common.Request;
using Newtonsoft.Json.Linq;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represent the response of a <see cref="GraphQLRequest"/>
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#response"/>
	/// </summary>
	public class GraphQLResponse : IEquatable<GraphQLResponse?> {

		/// <summary>
		/// The data of the response
		/// </summary>
		public dynamic? Data { get; set; }

		/// <summary>
		/// The Errors if occurred
		/// </summary>
		public GraphQLError[]? Errors { get; set; }

		/// <summary>
		/// Get a field of <see cref="Data"/> as Type
		/// </summary>
		/// <typeparam name="Type">The expected type</typeparam>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The field of data as an object</returns>
		public Type GetDataFieldAs<Type>(string fieldName) {
			var value = (this.Data as JObject?)!.GetValue(fieldName);
			return value.ToObject<Type>();
		}

		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse);

		/// <inheritdoc />
		public bool Equals(GraphQLResponse? other) {
			if (other == null) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			if (!Equals(this.Data, other.Data)) {
				return false;
			}
			if (!Equals(this.Errors, other.Errors)) {
				return false;
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode() => EqualityComparer<GraphQLResponse>.Default.GetHashCode(this);


		/// <inheritdoc />
		public static bool operator ==(GraphQLResponse? response1, GraphQLResponse? response2) => EqualityComparer<GraphQLResponse?>.Default.Equals(response1, response2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLResponse? response1, GraphQLResponse? response2) => !(response1 == response2);

	}

}
