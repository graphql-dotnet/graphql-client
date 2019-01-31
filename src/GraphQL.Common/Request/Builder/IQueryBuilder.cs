namespace GraphQL.Common.Request.Builder
{
	public interface IQueryBuilder
	{
		string Name { get; set; }
		string Build();
	}

	internal interface IQueryBuilderInternal : IQueryBuilder
	{
		IQueryBuilderInternal Parent { get; set; }
		IQueryBuilderInternal CurrentField { get; set; }
		IQueryBuilderInternal TryAddField(IQueryBuilder field);
		void AddParameter(QueryParameter parameter);
		void AddParameter(QueryParameterUsage parameter);
	}

	public interface IQueryBuilder<TEntity, out TProp, TParams> : IQueryBuilder
	{

	}
}
