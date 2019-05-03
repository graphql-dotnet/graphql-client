using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQL.Common.Request.Expression
{
	public class GraphQLExpressionNode
	{
		public string Name { get; set; }

		public List<GraphQLExpressionNode> Nodes { get; } = new List<GraphQLExpressionNode>();
		public string Alias { get; set; }

		public GraphQLExpressionNode(string name)
		{
			Name = name;
		}

		public GraphQLExpressionNode()
		{

		}


		public static GraphQLExpressionNode FromExpression(System.Linq.Expressions.Expression expression)
		{
			var node = new GraphQLExpressionNode();

			switch (expression)
			{
				case NewExpression newExpression:
				{
					for (var i = 0; i < newExpression.Arguments.Count; i++)
					{
						var argument = newExpression.Arguments[i];
						var expressionAlias = newExpression.Members[i].Name;


						var argNode = FromExpression(argument);
						argNode.Alias = expressionAlias;


						if (string.IsNullOrEmpty(argNode.ParentType) == false)
						{
							// TODO check if the parent type changes, if so throw
							node.Name = argNode.ParentType;
						}

						node.Nodes.Add(argNode);
					}
				}
					break;

				case MemberExpression memberExpression:
				{
					node.Name = memberExpression.Member.Name;
					var expressionString = memberExpression.ToString();
					var paths = expressionString.Split('.');

					if (paths.Count() > 2)
					{
						node.ParentType = paths[paths.Length - 2];
					}
				}
					break;

				case MethodCallExpression methodCallExpression:
				{
					// TODO catch arguments from here
					var firstArg = methodCallExpression.Arguments.First();
					var argNode = FromExpression(firstArg);

					var valueNode = FromExpression(methodCallExpression.Arguments.Last());

					node = valueNode;

					node.Name = argNode?.Name;
					node.Alias = argNode?.Alias;
				}
					break;

				case LambdaExpression lambdaExpression:
				{
					node = FromExpression(lambdaExpression.Body);
				}
					break;

				default:

					break;

			}


			return node;
		}

		public string ParentType { get; set; }
	}
}