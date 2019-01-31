using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Humanizer;

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

	public abstract class QueryBuilder : IQueryBuilder
	{
		protected readonly List<IQueryBuilder> _fields = new List<IQueryBuilder>();

		public string Name { get; set; }

		public IQueryBuilder Parent { get; set; }

		public IQueryBuilder CurrentField { get; set; }

		public QueryBuilder()
		{

		}

		protected QueryBuilder(QueryBuilder source)
		{
			_fields = new List<IQueryBuilder>(source._fields);
			CurrentField = source.CurrentField;
		}

		public string Build()
		{
			var root = Parent;
			while (root?.Parent != null)
			{
				root = root.Parent;
			}

			return root?.ToString() ?? ToString();
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			Build(stringBuilder, 0);

			return stringBuilder.ToString();
		}

		public abstract void Build(StringBuilder stringBuilder, int nestLevel);

		public IQueryBuilder TryAddField(IQueryBuilder field)
		{
			// TODO maybe replace with dictionary
			var found = _fields.FirstOrDefault(f => f.Name == field.Name);
			if (found == null)
			{
				_fields.Add(field);
				found = field;
			}

			return found;
		}
	}

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

	public class QueryBuilder<TEntity> : QueryBuilder, IQueryBuilder<TEntity>
	{
		private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
	
		public QueryBuilder()
		{
			Name = typeof(TEntity).Name;
		}

		protected QueryBuilder(QueryBuilder<TEntity> source)
			: base(source)
		{
			Name = typeof(TEntity).Name;
		}

		public override void Build(StringBuilder stringBuilder, int nestLevel)
		{
			const string space = "  ";

			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			nestLevel++;

			stringBuilder.Append($"{tab}{Name.Camelize()}");
			if (_fields.Any())
			{
				stringBuilder.AppendLine(" {");
				foreach (QueryBuilder field in _fields)
				{
					field.Build(stringBuilder, nestLevel);
				}
				stringBuilder.Append($"{tab}}}");
			}

			stringBuilder.AppendLine();
		}
	}

	public class QueryParameter
	{

	}

	public class QueryBuilder<TEntity, TProp> : QueryBuilder<TEntity>, IQueryBuilder<TEntity, TProp>
	{
		public QueryBuilder()
		{
		}

		internal QueryBuilder(QueryBuilder<TEntity> source)
			: base(source)
		{
		}
	}
}
