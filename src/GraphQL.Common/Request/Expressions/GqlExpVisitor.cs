using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace GraphQL.Common.Request.Expressions
{
	public class GqlExpVisitor : ExpressionVisitor
	{
		private (GqlVariable[] variables, string variableParameterName, object args, bool pretty, StringBuilder builder) _context;

		public GqlExpVisitor()
		{

		}


		protected string GqlExpression(string operation, Expression expression, string operationName = "",
			GqlVariable[] variables = null, string variableParameterName = "", object args = null, bool pretty = false)
		{
			var builder = new StringBuilder(5000);

			var variablesString = variables?.Any() == true
				? $"({string.Join(", ", variables.Select(x => x.ToString()))}) "
				: string.Empty;

			builder.AppendFormat("{0} {1}{2}{{ ", operation, operationName, variablesString);

			_context = (variables, variableParameterName, args, pretty, builder);
			var r = Visit(expression);

			builder.Append("}");
			var query = builder.ToString();
			builder.Clear();

			return query;
		}

		public string GetQuery(Expression expression, string operationName = "", GqlVariable[] variables = null, string variableParameterName = "", object args = null, bool pretty = false)
		{
			return GqlExpression("query", expression, operationName, variables, variableParameterName, args, pretty);
		}

		protected override Expression VisitNew(NewExpression node)
		{
			for (var i = 0; i < node.Arguments.Count; i++)
			{
				var argument = node.Arguments[i];
				var expressionAlias = node.Members[i].Name;
				var name = string.Empty;
				GqlParameter[] parameters = null;
				var hasChildren = argument.NodeType == ExpressionType.New || argument.NodeType == ExpressionType.Call;

				if (argument is MemberExpression member && member.Member.Name != expressionAlias)
				{
					name = member.Member.Name;
				}

				if (argument is NewExpression newExp)
				{
					foreach (var newExpArgument in newExp.Arguments)
					{
						var expressionString = newExpArgument is MethodCallExpression lambda ? lambda.Arguments[0].ToString() : newExpArgument.ToString();
						var paths = expressionString.Split('.');
						if (paths.Count() > 2)
						{
							if (string.IsNullOrEmpty(name))
							{
								name = paths[paths.Length - 2];
							}
						}

					}
				}

				if (argument is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Gql))
				{
					name = (methodCall.Arguments[0] as MemberExpression)?.Member.Name;
					parameters = GetParamsFromGql(methodCall).ToArray();
				}

				if (name?.ToCamelCase() == expressionAlias.ToCamelCase())
				{
					name = null;
				}


				_context.builder.AppendFormat("{0}{1}{2}{3}{4}{5}", _context.pretty ? "\t" : string.Empty, expressionAlias.ToCamelCase(),
					string.IsNullOrEmpty(name) ? string.Empty : $": {name.ToCamelCase()}",
					parameters?.Any() == true ? $" ({string.Join(", ", parameters.Select(x => x.ToParameter()))})" : string.Empty,
					hasChildren ? " {" : string.Empty, _context.pretty ? "\n\t" : " ");


				Visit(argument);

				if (hasChildren)
				{
					_context.builder.Append("} ");
				}
			}

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			return base.VisitMember(node);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.DeclaringType == typeof(Gql))
			{
				if (node.Method.Name == nameof(Gql.Field))
				{
					return Visit(node.Arguments[1]);

				}
			}
			return base.VisitMethodCall(node);
		}

		protected IEnumerable<GqlParameter> GetParamsFromGql(MethodCallExpression node)
		{
			if (node.Method.Name == nameof(Gql.Field))
			{
				var firstArg = node.Arguments.First();
				var name = ((MemberExpression)firstArg).Member.Name;


				if (node.Arguments.Count > 2)
				{
					return FromArgs(node
							.Arguments[2])
						.ToList();
				}

			}

			return new GqlParameter[0];
		}


		protected IEnumerable<GqlParameter> FromArgs(System.Linq.Expressions.Expression expression)
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

								if (typed.Name == _context.variableParameterName)
								{
									var xx = _context.variables.FirstOrDefault(x => x.Name == argumentMemberExpression.Member.Name);
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
		}
	}


}
