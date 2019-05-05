using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Common.Response;

namespace GraphQL.Common.Request.Expression
{
	public class GqlExpression<TType, TResponse>
		where TResponse : class
	{
		public GqlExpressionRoot Root { get; set; }
		public GqlVariable[] Variables { get; set; }


		public GraphQLRequest Build()
		{
			return new GraphQLRequest(Root.ToQuery(Variables));
		}

		public TResponse FromResponse(GraphQLResponse response)
		{
			return response.GetDataFieldAs<TResponse>(Root.Name);
		}
	}



	public static class Gql<TType>
	{
		public static GqlExpression<TType, TReturn> Query<TReturn, TArgs>(System.Linq.Expressions.Expression<Func<TType, TArgs, TReturn>> expression, TArgs args = default, string name = "")
			where TReturn : class
		{
			var variables = GqlVariable.FromArgs(args);
			var root = GqlExpressionNode.FromExpression(expression, new GqlExpressionNode.GqlExpressionContext()
			{
				Variables = variables,
				VariableParameterName = expression.Parameters.Last().Name,
				Args = args,
			});
			root.Name = name;

			return new GqlExpression<TType, TReturn>()
			{
				Root = new GqlExpressionRoot(root)
				{
					QueryType = "query"
				},
				Variables = variables
			};
		}

		public static GqlExpression<TType, TReturn> Query<TReturn>(System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, string name = "")
			where TReturn : class
		{
			var root = GqlExpressionNode.FromExpression(expression, null);
			root.Name = name;

			return new GqlExpression<TType, TReturn>()
			{
				Root = new GqlExpressionRoot(root),
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
		public static TReturn Field<TType, TReturn>(IEnumerable<TType> listField, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression)
			where TReturn : class
		{
			throw new InvalidOperationException();
		}

		public static TReturn Field<TType, TReturn, TArgs>(IEnumerable<TType> listField, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, TArgs args)
			where TReturn : class
		{
			throw new InvalidOperationException();
		}
		public static TReturn Field<TType, TReturn>(TType field, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression)
			where TReturn : class
		{
			throw new InvalidOperationException();
		}

		public static TReturn Field<TType, TReturn, TArgs>(TType field, System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, TArgs args)
			where TReturn : class
		{
			throw new InvalidOperationException();
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

		public override string ToString()
		{
			var required = IsRequired ? "!" : string.Empty;
			var type = string.IsNullOrEmpty(GqlType)
				? Value.GetType().GetGraphQLType()
				: GqlType;

			return $"${Name.ToCamelCase()}: {type}{required}";
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

			return $"{name}: {value}";
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
