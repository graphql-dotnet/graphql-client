using System.Linq;
using System.Text;

namespace GraphQL.Common.Request.Expression
{
	public class GraphQLExpressionNodeRoot : GraphQLExpressionNode
	{
		public string QueryType { get; set; } // Query / mutation

		public GraphQLExpressionNodeRoot()
		{

		}

		public GraphQLExpressionNodeRoot(GraphQLExpressionNode node)
		{
			Nodes.AddRange(node.Nodes);
		}
		public override string ToString()
		{
			var builder = new StringBuilder();
			Build(builder, this, 0);
			return builder.ToString();
		}

		public string ToQuery()
		{
			var builder = new StringBuilder();

			builder.Append($"{QueryType ?? "query"}");
			if (string.IsNullOrEmpty(Name) == false)
			{
				builder.Append($" {Name}");
			}

			// TODO parameters


			Build(builder, this, 1);

			return builder.ToString();
		}


		public void Build(StringBuilder builder, GraphQLExpressionNode node, int nestLevel)
		{
			const string space = "  ";
			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			++nestLevel;

			var name = node.Name?.ToCamelCase() ?? node.Alias?.ToCamelCase();

			var field = name == node.Alias?.ToCamelCase()
				? name.ToCamelCase()
				: $"{node.Alias}: {node.Name.ToCamelCase()}";

			builder.Append($"{tab}{field}");


			if (node.Nodes.Any())
			{
				builder.AppendLine(" {");

				foreach (var n in node.Nodes)
				{
					Build(builder, n, nestLevel);
				}

				builder.Append($"{tab}}}");
			}

			builder.AppendLine();
		}
	}
}