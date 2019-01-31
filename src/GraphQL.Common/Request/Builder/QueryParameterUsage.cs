using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public class QueryParameterUsage
	{
		public string Name { get; set; }
		public string InputName { get; set; }

		public override string ToString()
		{
			return $"{InputName}: ${Name.Camelize()}";
		}
	}
}