using System;
using System.Reflection;
using GraphQL.Common.Response;

namespace GraphQL.Common.Request.Expression
{
	public class GraphQLExpression<TType, TResponse>
		where TResponse : class
	{
		public GraphQLExpressionNodeRoot Root { get; set; }


		public GraphQLRequest Build()
		{
			return new GraphQLRequest(Root.ToQuery());
		}

		public TResponse FromResponse(GraphQLResponse response)
		{
			return response.GetDataFieldAs<TResponse>(Root.Name);
		}
	}

	public static class GraphQLExpression<TType>
	{
		public static GraphQLExpression<TType, TReturn> Build<TReturn>(System.Linq.Expressions.Expression<Func<TType, TReturn>> expression, string name = "")
			where TReturn : class
		{
			var root = GraphQLExpressionNode.FromExpression(expression);
			root.Name = name;

			return new GraphQLExpression<TType, TReturn>()
			{
				Root = new GraphQLExpressionNodeRoot(root)
			};
		}
	}



	/// <summary>
	///  From https://github.com/graphql-dotnet/graphql-client/pull/93
	/// </summary>
	public class QueryParameter
	{
		public Type Type { get; set; }
		public string Name { get; set; }
		public bool IsRequired { get; set; }

		public override string ToString()
		{
			var required = IsRequired ? "!" : string.Empty;
			return $"${Name.ToCamelCase()}: {Type.GetGraphQLType()}{required}";
		}
	}

	public static class GraphQLHelper
	{

		public static TResult WithParameters<TType, TResult>(this TType x, System.Linq.Expressions.Expression<Func<TType, TResult>> expression, params QueryParameter[] parameters)
		{
			return default(TResult);
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

			var graphQLTypeAttribute = (GraphQLTypeAttribute) type.GetCustomAttribute(typeof(GraphQLTypeAttribute));
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
