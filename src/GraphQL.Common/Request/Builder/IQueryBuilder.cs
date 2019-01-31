namespace GraphQL.Common.Request.Builder
{
	public interface IQueryBuilder
	{
		string Name { get; set; }
		IQueryBuilder Parent { get; set; }
		IQueryBuilder CurrentField { get; set; }
		string Build();
		IQueryBuilder TryAddField(IQueryBuilder field);
	}

	public interface IQueryBuilder<out TEntity> : IQueryBuilder
	{

	}

	public interface IQueryBuilder<out TEntity, out TProp> : IQueryBuilder<TEntity>
	{

	}
}
