#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Common.Request;
using Newtonsoft.Json.Linq;

namespace GraphQL.Common.Response {

	/// <summary>
	/// Represent the response of a <see cref="GraphQLRequest"/>
	/// For more information <see href="http://graphql.org/learn/serving-over-http/#response"/>
	/// </summary>
	public class GraphQLResponse : GraphQLResponse<dynamic> {
		/// <summary>
		/// Get a field of <see cref="GraphQLResponse{dynamic}.Data"/> as Type
		/// </summary>
		/// <typeparam name="TType">The expected type</typeparam>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The field of data as an object</returns>
		public TType? GetDataFieldAs<TType>(string fieldName) where TType:class
		{
			if(this.Data is JObject jObjectData) {
				return jObjectData.GetValue(fieldName).ToObject<TType>();
			}
			return (TType?)this.Data?.GetType()
				.GetProperty(fieldName)
				.GetValue(this.Data, null);
		}
	}


	public class GraphQLResponse<TData> : IEquatable<GraphQLResponse<TData>?>
		where TData: class
	{
		/// <summary>
		/// The data of the response
		/// </summary>
		public TData? Data { get; set; }

		/// <summary>
		/// The Errors if occurred
		/// </summary>
		public GraphQLError[]? Errors { get; set; }


		/// <inheritdoc />
		public override bool Equals(object? obj) => this.Equals(obj as GraphQLResponse<TData>);

		/// <inheritdoc />
		public bool Equals(GraphQLResponse<TData>? other)
		{
			if (other == null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			if (!EqualityComparer<TData?>.Default.Equals(this.Data, other.Data)) { return false; }
			{
				if (this.Errors != null && other.Errors != null)
				{
					if (!Enumerable.SequenceEqual(this.Errors, other.Errors)) { return false; }
				}
				else if (this.Errors != null && other.Errors == null) { return false; }
				else if (this.Errors == null && other.Errors != null) { return false; }
			}
			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = EqualityComparer<TData?>.Default.GetHashCode(this.Data);
				{
					if (this.Errors != null)
					{
						foreach (var element in this.Errors)
						{
							hashCode = (hashCode * 397) ^ EqualityComparer<GraphQLError?>.Default.GetHashCode(element);
						}
					}
					else
					{
						hashCode = (hashCode * 397) ^ 0;
					}
				}
				return hashCode;
			}
		}


		/// <inheritdoc />
		public static bool operator ==(GraphQLResponse<TData>? response1, GraphQLResponse<TData>? response2) => EqualityComparer<GraphQLResponse<TData>?>.Default.Equals(response1, response2);

		/// <inheritdoc />
		public static bool operator !=(GraphQLResponse<TData>? response1, GraphQLResponse<TData>? response2) => !(response1 == response2);
	}

}
