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
			IQueryBuilder field = new QueryBuilder<TEntity, TProp>
			{
				Name = propertyName,
				Parent = newBuilder
			};

			field = newBuilder.TryAddField(field);
			newBuilder.CurrentField = field;

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
			IQueryBuilder field = new QueryBuilder<TEntity, TProp>
			{
				Name = propertyName,
				Parent = newBuilder
			};

			field = newBuilder.CurrentField.TryAddField(field);
			newBuilder.CurrentField = field;

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
			IQueryBuilder field = new QueryBuilder<TEntity, TChild>
			{
				Name = propertyName,
				Parent = builder
			};

			field = newBuilder.CurrentField.TryAddField(field);
			newBuilder.CurrentField = field;

			return newBuilder;
		}
	}
}