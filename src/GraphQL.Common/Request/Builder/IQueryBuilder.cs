namespace GraphQL.Common.Request.Builder
{
	public interface IQueryBuilder
	{
		string Build();
	}

	internal interface IQueryBuilderInternal : IQueryBuilder
	{
		string OperationName { get; set; }
		string Name { get; set; }
		IQueryBuilderInternal CurrentField { get; set; }
		IQueryBuilderInternal TryAddField(IQueryBuilderInternal field);
		void AddParameter(QueryParameter parameter);
		void AddParameter(QueryParameterUsage parameter);
	}

	public interface IQueryBuilder<TEntity, out TProp, TParams> : IQueryBuilder
	{

	}
}
