using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphQL.Common.Request.Builder
{
	public static class QueryBuilderExtensions
	{
		public static IQueryBuilder<TEntity, TProp, TParams> Include<TEntity, TOldProp, TProp, TParams>(this IQueryBuilder<TEntity, TOldProp, TParams> builder, Expression<Func<TEntity, TProp>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = body.Member.Name;

			var newBuilder = new QueryBuilder<TEntity, TProp, TParams>((QueryBuilder) builder);
			var newInternal = (IQueryBuilderInternal) newBuilder;
			IQueryBuilderInternal field = new QueryBuilder
			{
				Name = propertyName
			};
			field.Parent = newBuilder;

			field = newInternal.TryAddField(field);
			newInternal.CurrentField = field;

			return newBuilder;
		}

		public static IQueryBuilder<TEntity, TProp, TParams> ThenInclude<TEntity, TProp, TParams, TChild>(this IQueryBuilder<TEntity, TProp, TParams> builder, Expression<Func<TProp, TChild>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = property.Name;

			var newBuilder = new QueryBuilder<TEntity, TProp, TParams>((QueryBuilder)builder);
			var newInternal = (IQueryBuilderInternal)newBuilder;
			IQueryBuilderInternal field = new QueryBuilder
			{
				Name = propertyName
			};
			field.Parent = newBuilder;

			field = newInternal.CurrentField.TryAddField(field);
			newInternal.CurrentField = field;

			return newBuilder;
		}

		public static IQueryBuilder<TEntity, TChild, TParams> ThenInclude<TEntity, TProp, TParams, TChild>(this IQueryBuilder<TEntity, IEnumerable<TProp>, TParams> builder, Expression<Func<TProp, TChild>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(propertyExpression));
			var propertyName = property.Name;

			var newBuilder = new QueryBuilder<TEntity, TChild, TParams>((QueryBuilder) builder);
			var newInternal = (IQueryBuilderInternal) newBuilder;
			IQueryBuilderInternal field = new QueryBuilder
			{
				Name = propertyName
			};
			field.Parent = (IQueryBuilderInternal) builder;

			field = newInternal.CurrentField.TryAddField(field);
			newInternal.CurrentField = field;

			return newBuilder;
		}

		public static IQueryBuilder<TEntity, TProp, TParams> UseParameter<TEntity, TProp, TParams, TParam>(this IQueryBuilder<TEntity, TProp, TParams> builder, string inputName, Expression<Func<TParams, TParam>> parameterExpression)
		{
			var body = parameterExpression.Body as MemberExpression;
			var property = body?.Member as PropertyInfo;
			if (property == null)
				throw new ArgumentException("Expression must indicate a property.", nameof(parameterExpression));
			var parameterName = property.Name;

			var builderInternal = (IQueryBuilderInternal) builder;
			builderInternal.CurrentField.AddParameter(new QueryParameterUsage
			{
				Name = parameterName,
				InputName = inputName
			});

			return builder;
		}

		public static IQueryBuilder<TEntity, TProp, TParams> WithParameters<TEntity, TProp, TParams>(this IQueryBuilder<TEntity, TProp, object> builder, TParams parameters)
		{
			var newBuilder = new QueryBuilder<TEntity, TProp, TParams>((QueryBuilder)builder);
			var newInternal = (IQueryBuilderInternal)newBuilder;

			var properties = typeof(TParams).GetProperties();
			foreach (var property in properties)
			{
				newInternal.AddParameter(new QueryParameter
				{
					Type = property.PropertyType,
					Name = property.Name,
					IsRequired = true
				});
			}


			return newBuilder;
		}
	}
}
