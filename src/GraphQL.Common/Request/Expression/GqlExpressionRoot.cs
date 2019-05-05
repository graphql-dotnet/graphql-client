using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace GraphQL.Common.Request.Expression
{
	public class GqlExpressionRoot : GqlExpressionNode
	{
		public string QueryType { get; set; } // Query / mutation

		public GqlExpressionRoot()
		{

		}

		public GqlExpressionRoot(GqlExpressionNode node)
		{
			Nodes.AddRange(node.Nodes);
		}
		public override string ToString()
		{
			var builder = new StringBuilder();
			Build(builder, this, 0);
			return builder.ToString();
		}

		public string ToQuery(GqlVariable[] variables)
		{
			var builder = new StringBuilder();

			builder.Append($"{QueryType ?? "query"}");
			if (string.IsNullOrEmpty(Name) == false)
			{
				builder.Append($" {Name}");
			}

			if (variables?.Any() == true)
			{
				builder.Append(" (");
				builder.Append(string.Join(", ", variables.Select(x=>x.ToString())));
				builder.Append(")");
			}


			Build(builder, this, 1);

			return builder.ToString();
		}

		public void Build(StringBuilder builder, GqlExpressionNode node, int nestLevel)
		{
			const string space = "  ";
			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			++nestLevel;

			var name = node.Name?.ToCamelCase() ?? node.Alias?.ToCamelCase();

			var field = name == node.Alias?.ToCamelCase()
				? name.ToCamelCase()
				: $"{node.Alias}: {node.Name.ToCamelCase()}";

			builder.Append($"{tab}{field}");
			if (node.Parameters?.Any() == true)
			{
				builder.Append(" (");
				builder.Append(string.Join(", ", node.Parameters.Select(x => x.ToParameter())));
				builder.Append(")");
			}


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
