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
	}

	public interface IQueryBuilder<out TEntity> : IQueryBuilder
	{

	}

	public interface IQueryBuilder<out TEntity, out TProp> : IQueryBuilder<TEntity>
	{

	}
}
