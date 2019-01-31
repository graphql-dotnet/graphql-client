using System;
using System.Reflection;

namespace GraphQL.Common.Request.Builder
{
	public static class TypeExtensions
	{
		public static string GetGraphQLType(this Type type)
		{
			if (type == typeof(int)) return "Int";
			if (type == typeof(double)) return "Float";
			if (type == typeof(string)) return "String";
			if (type == typeof(bool)) return "Boolean";
			if (type == typeof(Guid)) return "ID";

			var graphQLTypeAttribute = (GraphQLTypeAttribute) type.GetCustomAttribute(typeof(GraphQLTypeAttribute));
			if (graphQLTypeAttribute != null)
				return graphQLTypeAttribute.TypeName;

			return type.Name;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class GraphQLTypeAttribute : Attribute
	{
		public string TypeName { get; }

		public GraphQLTypeAttribute(string typeName)
		{
			TypeName = typeName;
		}
	}
}
