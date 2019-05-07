using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQL.Common.Request.Expressions
{
	public class GqlExpressionNode
	{
		public GqlExpressionNode(string name)
		{
			Name = name;
		}

		public GqlExpressionNode()
		{
		}

		public string Name { get; set; }

		public List<GqlExpressionNode> Nodes { get; } = new List<GqlExpressionNode>();
		public string Alias { get; set; }

		public string ParentType { get; set; }


		public class GqlExpressionContext
		{
			public GqlVariable[] Variables { get; set; }

			public string VariableParameterName { get; set; }


			public object Args { get; set; }
		}

		public static GqlExpressionNode FromExpression(System.Linq.Expressions.Expression expression, GqlExpressionContext context)
		{
			var node = new GqlExpressionNode();

			switch (expression)
			{
				case NewExpression newExpression:
					{
						for (var i = 0; i < newExpression.Arguments.Count; i++)
						{
							var argument = newExpression.Arguments[i];
							var expressionAlias = newExpression.Members[i].Name;


							var argNode = FromExpression(argument, context);
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
						if (methodCallExpression.Method.DeclaringType == typeof(Gql))
						{
							if (methodCallExpression.Method.Name == nameof(Gql.Field))
							{
								var firstArg = methodCallExpression.Arguments.First();
								var name = ((MemberExpression)firstArg).Member.Name;

								var expressionArg =
									(methodCallExpression.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;

								var valueNode = FromExpression(expressionArg.Body, context);
								node = valueNode;
								node.Name = name;


								if (methodCallExpression.Arguments.Count > 2)
								{
									var args = FromArgs(methodCallExpression
										.Arguments[2], context)
										.ToList();
									node.Parameters = args;
								}
							}
						}
						else
						{
							var firstArg = methodCallExpression.Arguments.First();
							var argNode = FromExpression(firstArg, context);

							var valueNode = FromExpression(methodCallExpression.Arguments.Last(), context);

							node = valueNode;

							node.Name = argNode?.Name;
							node.Alias = argNode?.Alias;
						}
					}
					break;

				case LambdaExpression lambdaExpression:
					{
						node = FromExpression(lambdaExpression.Body, context);
					}
					break;
			}


			return node;
		}

		public List<GqlParameter> Parameters { get; set; }

		public static IEnumerable<GqlParameter> FromArgs(System.Linq.Expressions.Expression expression, GqlExpressionContext context)
		{

			switch (expression)
			{
				case NewExpression newExpression:
					{
						for (var i = 0; i < newExpression.Arguments.Count; i++)
						{
							var argument = newExpression.Arguments[i];
							var expressionAlias = newExpression.Members[i].Name;

							if (argument is MemberExpression argumentMemberExpression)
							{
								var typed = (ParameterExpression)argumentMemberExpression.Expression;

								if (typed.Name == context.VariableParameterName)
								{
									var xx = context.Variables.FirstOrDefault(x => x.Name == argumentMemberExpression.Member.Name);
									yield return new GqlParameter()
									{
										Name = expressionAlias,
										Value = xx
									};
								}
								else
								{
									//TODO probably compile!?
									throw new Exception("Not supported");
								}
							}
							else if (argument is ConstantExpression constantExpression)
							{
								yield return new GqlParameter()
								{
									Name = expressionAlias,
									Value = constantExpression.Value
								};
							}
							else
							{
								throw new Exception("Not supported");
							}

						}

						break;
					}

			}
			yield break;

			//			return null;
		}
	}

}
