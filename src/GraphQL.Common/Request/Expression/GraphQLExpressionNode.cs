using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQL.Common.Request.Expression
{
	public class GraphQLExpressionNode
	{
		public GraphQLExpressionNode(string name)
		{
			Name = name;
		}

		public GraphQLExpressionNode()
		{
		}

		public string Name { get; set; }

		public List<GraphQLExpressionNode> Nodes { get; } = new List<GraphQLExpressionNode>();
		public string Alias { get; set; }

		public GraphQLParameter[] Parameters { get; set; }

		public string ParentType { get; set; }


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


						if (string.IsNullOrEmpty(argNode.ParentType) == false) node.Name = argNode.ParentType;

						node.Nodes.Add(argNode);
					}
				}
					break;

				case MemberExpression memberExpression:
				{
					node.Name = memberExpression.Member.Name;
					var expressionString = memberExpression.ToString();
					var paths = expressionString.Split('.');

					if (paths.Count() > 2) node.ParentType = paths[paths.Length - 2];
				}
					break;

				case MethodCallExpression methodCallExpression:
				{
//					if (methodCallExpression.Method.Name == nameof(GqlExp.WithParameters))
					if (methodCallExpression.Method.DeclaringType == typeof(GqlExp))
					{
						System.Linq.Expressions.Expression typeArgument;
						LambdaExpression resultArgument;
						LambdaExpression parametersArgument;
						switch (methodCallExpression.Method.Name)
						{
							case nameof(GqlExp.WithParameters):
							{
								typeArgument = methodCallExpression.Arguments[0];
								resultArgument =
									(methodCallExpression.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
								parametersArgument =
									System.Linq.Expressions.Expression.Lambda(methodCallExpression.Arguments[2]);
								break;
							}
							case nameof(GqlExp.Params):
							{
								typeArgument = methodCallExpression.Arguments[1];
								resultArgument =
									(methodCallExpression.Arguments[2] as UnaryExpression)?.Operand as LambdaExpression;
								parametersArgument =
									System.Linq.Expressions.Expression.Lambda(methodCallExpression.Arguments[0]);
								break;
							}
							default:
								throw new ArgumentOutOfRangeException();
						}

						var argNode = FromExpression(typeArgument);
						var valueNode = FromExpression(resultArgument?.Body);
						node = valueNode;

						node.Name = argNode?.Name;
						node.Alias = argNode?.Alias;

						var method = parametersArgument.Compile();
						var parameters = method.DynamicInvoke() as GraphQLParameter[];

						node.Parameters = parameters;
					}
					else
					{
						var firstArg = methodCallExpression.Arguments.First();
						var argNode = FromExpression(firstArg);

						var valueNode = FromExpression(methodCallExpression.Arguments.Last());

						node = valueNode;

						node.Name = argNode?.Name;
						node.Alias = argNode?.Alias;
					}
				}
					break;

				case LambdaExpression lambdaExpression:
				{
					node = FromExpression(lambdaExpression.Body);
				}
					break;
			}


			return node;
		}
	}
}
