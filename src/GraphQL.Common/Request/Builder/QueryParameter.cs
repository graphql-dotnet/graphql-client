using System;
using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public class QueryParameter
	{
		public Type Type { get; set; }
		public string Name { get; set; }
		public bool IsRequired { get; set; }

		public override string ToString()
		{
			var required = IsRequired ? "!" : string.Empty;
			return $"${Name.Camelize()}: {Type.GetGraphQLType()}{required}";
		}
	}

	public static class TypeExtensions
	{
		public static string GetGraphQLType(this Type type)
		{
			if (type == typeof(int)) return "Int";
			if (type == typeof(double)) return "Float";
			if (type == typeof(string)) return "String";
			if (type == typeof(bool)) return "Boolean";
			if (type == typeof(Guid)) return "ID";

			throw new NotImplementedException($"Type {type} cannot be translated.");
		}
	}
}
