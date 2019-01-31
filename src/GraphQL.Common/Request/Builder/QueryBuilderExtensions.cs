using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphQL.Common.Request.Builder
{
	public static class QueryBuilderExtensions
	{
		public static IQueryBuilder<TEntity, TProp> Include<TEntity, TProp>(this IQueryBuilder<TEntity> builder, Expression<Func<TEntity, TProp>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = body.Member.Name;

			var newBuilder = new QueryBuilder<TEntity, TProp>((QueryBuilder<TEntity>) builder);
			var newInternal = (IQueryBuilderInternal) newBuilder;
			IQueryBuilderInternal field = new QueryBuilder<TEntity, TProp>
			{
				Name = propertyName
			};
			field.Parent = newBuilder;

			field = newInternal.TryAddField(field);
			newInternal.CurrentField = field;

			return newBuilder;
		}

		public static IQueryBuilder<TEntity, TProp> ThenInclude<TEntity, TProp, TChild>(this IQueryBuilder<TEntity, TProp> builder, Expression<Func<TProp, TChild>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = body.Member.Name;

			var newBuilder = new QueryBuilder<TEntity, TProp>((QueryBuilder<TEntity>) builder);
			var newInternal = (IQueryBuilderInternal) newBuilder;
			IQueryBuilderInternal field = new QueryBuilder<TEntity, TProp>
			{
				Name = propertyName
			};
			field.Parent = newBuilder;

			field = newInternal.CurrentField.TryAddField(field);
			newInternal.CurrentField = field;

			return newBuilder;
		}

		public static IQueryBuilder<TEntity, TChild> ThenInclude<TEntity, TProp, TChild>(this IQueryBuilder<TEntity, IEnumerable<TProp>> builder, Expression<Func<TProp, TChild>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = body.Member.Name;

			var newBuilder = new QueryBuilder<TEntity, TChild>((QueryBuilder<TEntity>) builder);
			var newInternal = (IQueryBuilderInternal) newBuilder;
			IQueryBuilderInternal field = new QueryBuilder<TEntity, TChild>
			{
				Name = propertyName
			};
			field.Parent = (IQueryBuilderInternal) builder;

			field = newInternal.CurrentField.TryAddField(field);
			newInternal.CurrentField = (IQueryBuilderInternal) field;

			return newBuilder;
		}
	}
}
