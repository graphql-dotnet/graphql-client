using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public abstract class QueryBuilder : IQueryBuilder
	{
		protected readonly List<IQueryBuilder> _fields = new List<IQueryBuilder>();

		public string Name { get; set; }

		public IQueryBuilder Parent { get; set; }

		public IQueryBuilder CurrentField { get; set; }

		protected QueryBuilder()
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
