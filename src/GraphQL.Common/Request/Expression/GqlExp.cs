using System;
using System.Linq;
using System.Reflection;
using GraphQL.Common.Response;

namespace GraphQL.Common.Request.Expression
{
	public class GraphQLExpression<TType, TResponse>
		where TResponse : class
	{
		public GraphQLExpressionNodeRoot Root { get; set; }
		public GraphQLParameter[] Parameters { get; set; }

		public GqlVariable[] Variables { get; set; }


		public GraphQLRequest Build()
		{
			if (Variables != null)
			{
				Parameters = Variables.Select(x => new GraphQLParameter()
				{
					IsRequired = x.IsRequired,
					Value = x.Value,
					Name = x.Name,
					GraphQLType = x.GqlType
				}).ToArray();
			}

			return new GraphQLRequest(Root.ToQuery(Parameters));
		}

		public TResponse FromResponse(GraphQLResponse response)
		{
			return response.GetDataFieldAs<TResponse>(Root.Name);
		}
	}

	

	public static class Gql<TType>
	{
		public static GraphQLExpression<TType, TReturn> Query<TReturn, TArgs>(System.Linq.Expressions.Expression<Func<TType, TArgs, TReturn>> expression, TArgs args = default, string name = "")
			where TReturn : class
		{
			var variables = GqlVariable.FromArgs(args);
			var root = GraphQLExpressionNode.FromExpression(expression, new GraphQLExpressionNode.GqlExpressionContext()
			{
				Variables = variables,
				VariableParameterName = expression.Parameters.Last().Name,

				Args = args,
			});
			
			root.Name = name;

			return new GraphQLExpression<TType, TReturn>()
			{
				Root = new GraphQLExpressionNodeRoot(root),
				Variables = variables

			};
		}

		public static GraphQLExpression<TType, TReturn> Query<TReturn>(System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, string name = "")
			where TReturn : class
		{
			var root = GraphQLExpressionNode.FromExpression(expression, null);
			root.Name = name;

			return new GraphQLExpression<TType, TReturn>()
			{
				Root = new GraphQLExpressionNodeRoot(root),
				//ParametersOld = parameters
			};
		}

		public static TReturn Field<TReturn>(System.Linq.Expressions.Expression<Func<TType, TReturn>> expression)
			where TReturn : class
		{
			return null;
		}

		public static TReturn Field<TReturn, TArgs>(System.Linq.Expressions.Expression<Func<TType, TArgs, TReturn>> expression, TArgs args)
			where TReturn : class
		{
			return null;
		}
	}

	public static class Gql
	{
		public static TReturn Field<TType, TReturn>(TType field, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression)
			where TReturn : class
		{
			return null;
		}

		public static TReturn Field<TType, TReturn, TArgs>(TType field, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, TArgs args)
			where TReturn : class
		{
			return null;
		}
	}



	public static class GqlExp<TType>
	{

		public static GraphQLExpression<TType, TReturn> Build<TReturn>(
			GraphQLParameter[] parameters,
			System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, string name = "")
			where TReturn : class
		{
			var root = GraphQLExpressionNode.FromExpression(expression, null);
			root.Name = name;

			return new GraphQLExpression<TType, TReturn>()
			{
				Root = new GraphQLExpressionNodeRoot(root),
				Parameters = parameters
			};
		}


		public static GraphQLExpression<TType, TReturn> Build<TReturn>(System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, string name = "")
			where TReturn : class
		{
			return Build(new GraphQLParameter[0], expression, name);
		}
	}


	public class GqlVariable
	{
		public string Name { get; set; }
		public bool IsRequired { get; set; }

		public object Value { get; set; }

		public string GqlType { get; set; }

		public static GqlVariable[] FromArgs<TArgs>(TArgs args)
		{
			return args.GetType().GetProperties()
				.Select(x =>
				{
					var value = x.GetValue(args);
					if (value is GqlVariable variable)
					{
						return variable;
					}

					if (value is GraphQLParameter param)
					{
						return new GqlVariable()
						{
							Name = param.Name,
							Value = param.Value,
							IsRequired = param.IsRequired,
							GqlType = param.GetGqlType()

						};
					}

					var nullableType = Nullable.GetUnderlyingType(x.PropertyType);

					return new GqlVariable()
					{
						Name = x.Name,
						IsRequired = nullableType == null,
						Value = value,
						GqlType = (nullableType ?? x.PropertyType).GetGraphQLType()
					};
				}).ToArray();

		}
	}

	public class GqlParameter
	{
		public string Name { get; set; }

		public object Value { get; set; }

		public string ToParameter()
		{
			var name = Name.ToCamelCase();

			var value = Value is GqlVariable variable
				? $"${variable.Name}"
				: (Value is string ? $"\"{Value}\"" : Value);

//			var value = string.IsNullOrEmpty(ParameterValue?.Name) ? (Value is string ? $"\"{Value}\"" : Value)
//				: $"${ParameterValue.Name}";

			return $"{name}: {value}";
		}
	}

	public class GraphQLParameter
	{
		public string Name { get; set; }
		public bool IsRequired { get; set; }

		public object Value { get; set; }

		public Type Type { get; set; }
		public string GraphQLType { get; set; }

		public GraphQLParameter ParameterValue { get; set; }


		public GraphQLParameter ToArgument()
		{
			return new GraphQLParameter()
			{
				Name = Name,
				ParameterValue = this,
				Type = Type
			};
		}

		public static GraphQLParameter Build(string name, bool isRequired = false)
		{
			return new GraphQLParameter()
			{
				Name = name,
				IsRequired = isRequired,
			};
		}

		public static GraphQLParameter Build<TType>(string name, bool isRequired = false,
			TType value = default, string graphQlType = "")
		{
			return new GraphQLParameter()
			{
				Name = name,
				IsRequired = isRequired,
				Value = value,
				GraphQLType = graphQlType,
				Type = typeof(TType)
			};
		}

		public static GraphQLParameter Build(string name, GraphQLParameter parameter)
		{
			return new GraphQLParameter()
			{
				Name = name,
				ParameterValue = parameter
			};
		}

		public string GetGqlType()
		{
			return string.IsNullOrEmpty(GraphQLType)
				? Type.GetGraphQLType()
				: GraphQLType;
		}

		public string ToQueryParameter()
		{
			var required = IsRequired ? "!" : string.Empty;

			var type = string.IsNullOrEmpty(GraphQLType)
				? Type.GetGraphQLType()
				: GraphQLType;

			return $"${Name.ToCamelCase()}: {type}{required}";
		}

		public string ToParameter()
		{
			var name = Name.ToCamelCase();
			var value = string.IsNullOrEmpty(ParameterValue?.Name) ? (Value is string ? $"\"{Value}\"" : Value)
			: $"${ParameterValue.Name}";

			return $"{name}: {value}";
		}

		public override string ToString()
		{
			var required = IsRequired ? "!" : string.Empty;
			var name = Name.ToCamelCase();
			var type = GraphQLType ?? Type?.GetGraphQLType() ?? string.Empty;


			return $"${Name.ToCamelCase()}: {GraphQLType ?? Type.GetGraphQLType()}{required}";
		}
	}



	public static class GqlExp
	{


		public static TResult Params<TType, TResult>(GraphQLParameter[] parameters, TType x,
			System.Linq.Expressions.Expression<Func<TType, TResult>> expression)
		{
			throw new InvalidOperationException("This should only be used by the Expression Builder");
		}


		public static TResult WithParameters<TType, TResult>(this TType x, System.Linq.Expressions.Expression<Func<TType, TResult>> expression, params GraphQLParameter[] parameters)
		{
			throw new InvalidOperationException("This should only be used by the Expression Builder");
		}



	}

	public static class TypeExtensions
	{
		public static string GetGraphQLType(this Type type)
		{
			if (type == typeof(int)) return "Int";
			if (type == typeof(double)) return "Float";
			if (type == typeof(string)) return "String";
			if (type == typeof(bool)) return "Boolean";
			if (type == typeof(Guid)) return "ID";

			var graphQLTypeAttribute = (GraphQLTypeAttribute)type.GetCustomAttribute(typeof(GraphQLTypeAttribute));
			if (graphQLTypeAttribute != null)
				return graphQLTypeAttribute.TypeName;

			return type.Name;
		}

	}


	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class GraphQLTypeAttribute : Attribute
	{
		public string TypeName { get; }

		public GraphQLTypeAttribute(string typeName)
		{
			TypeName = typeName;
		}
	}

}
