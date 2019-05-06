using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using GraphQL.Common.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

	public class GqlExpression<TType, TResponse, TArgs>
		where TResponse : class
	{
		public GqlExpressionRoot Root { get; set; }
		public GqlVariable[] Variables { get; set; }


		public GraphQLRequest Build(TArgs variables = default)
		{
			return new GraphQLRequest(Root.ToQuery(Variables))
			{
				Variables = variables
			};
		}


		public TResponse FromResponse(GraphQLResponse response)
		{
			return response.GetDataFieldAs<TResponse>(Root.Name);
		}
	}



	public static class Gql<TType>
	{
		public static GqlExpression<TType, TReturn, TArgs> Query<TReturn, TArgs>(System.Linq.Expressions.Expression<Func<TType, TArgs, TReturn>> expression, TArgs args = default, string name = "")
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

			return new GqlExpression<TType, TReturn, TArgs>()
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
				Root = new GqlExpressionRoot(root)
				{
					QueryType = "query"
				},
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


	[JsonConverter(typeof(NiceDateConverter))]
	public class GqlID<T>
		where T : struct
	{
		public T Value { get; }

		public GqlID(T value)
		{
			Value = value;
		}

		//public void SetValue(object target, object value)
		//{
		//	Value = (T)value;
		//}

		//public object GetValue(object target)
		//{
		//	return Value;
		//}
	}

	public class NiceDateConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as dynamic;
			writer.WriteValue(v.Value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType && objectType.GetGenericTypeDefinition().IsAssignableFrom(typeof(GqlID<>));
		}
	}

	public static class GqlID
	{
		public static GqlID<T> From<T>(T v)
			where T : struct
		{
			return new GqlID<T>(v);
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

			if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(GqlID<>))) return "ID";

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
