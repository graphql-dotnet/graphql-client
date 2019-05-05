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

		public GraphQLParameter[] ParametersOld { get; set; }

		public string ParentType { get; set; }


		public class GqlExpressionContext
		{
			public GqlVariable[] Variables { get; set; }

			public string VariableParameterName { get; set; }


			public object Args { get; set; }
		}

		public static GraphQLExpressionNode FromExpression(System.Linq.Expressions.Expression expression, GqlExpressionContext context)
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

							var argNode = FromExpression(typeArgument, context);
							var valueNode = FromExpression(resultArgument?.Body, context);
							node = valueNode;

							node.Name = argNode?.Name;
							node.Alias = argNode?.Alias;

							var method = parametersArgument.Compile();
							var parameters = method.DynamicInvoke() as GraphQLParameter[];

							node.ParametersOld = parameters;
						}
						else if (methodCallExpression.Method.DeclaringType == typeof(Gql))
						{
							if (methodCallExpression.Method.Name == nameof(Gql.Field))
							{
								var firstArg = methodCallExpression.Arguments.First();
								var name = ((MemberExpression)firstArg).Member.Name;

								var expressionArg =
									(methodCallExpression.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;

								//var argNode = FromExpression(firstArg, context);

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


							//						var argNode = FromExpression(argument, context);
							//						argNode.Alias = expressionAlias;


							//						if (string.IsNullOrEmpty(argNode.ParentType) == false) node.Name = argNode.ParentType;
							//
							//						node.Nodes.Add(argNode);
						}

						break;
					}

			}
			yield break;

			//			return null;
		}
	}

}
