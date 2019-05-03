using System;
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

}
