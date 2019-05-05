using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

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

		public string ToQuery(GraphQLParameter[] parameters)
		{
			var builder = new StringBuilder();

			builder.Append($"{QueryType ?? "query"}");
			if (string.IsNullOrEmpty(Name) == false)
			{
				builder.Append($" {Name}");
			}

			if (parameters?.Any() == true)
			{
				builder.Append(" (");
				builder.Append(string.Join(", ", parameters.Select(x => x.ToQueryParameter())));
				builder.Append(")");
			}


			Build(builder, this, 1);

			return builder.ToString();
		}


		public IEnumerable<string> BuildLines(GraphQLExpressionNode node, int nestLevel, bool pretty = true)
		{
			const string space = "  ";
			var tab = pretty ?  string.Join(string.Empty, Enumerable.Repeat(space, nestLevel)) : " ";
			++nestLevel;
			var name = node.Name?.ToCamelCase() ?? node.Alias?.ToCamelCase();

			var field = name == node.Alias?.ToCamelCase()
				? name.ToCamelCase()
				: $"{node.Alias}: {node.Name.ToCamelCase()}";


			var parameters = "";

			if(node.ParametersOld?.Any() == true)
			{
				parameters = $" ({string.Join(", ", node.ParametersOld.Select(x => x.ToParameter()))}) ";
			}


			if (node.Nodes.Any())
			{
				yield return $"{tab}{field}{parameters} {{";

				foreach (var n in node.Nodes.SelectMany(x=>BuildLines(x, nestLevel, pretty)))
				{
					yield return n;
				}

				yield return $"{tab}}}";
			}
			else
			{
				yield return $"{tab}{field}{parameters}";
			}
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

			if (node.ParametersOld?.Any() == true)
			{
				builder.Append(" (");
				builder.Append(string.Join(", ", node.ParametersOld.Select(x => x.ToParameter())));
				builder.Append(")");
			}

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
