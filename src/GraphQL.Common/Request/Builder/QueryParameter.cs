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
}
