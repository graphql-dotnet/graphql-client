using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public class QueryBuilder<TEntity>
	{
		private readonly List<QueryBuilder<TEntity>> _fields = new List<QueryBuilder<TEntity>>();
		private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
		private string _name;

		public string Name
		{
			get => _name ?? (_name = typeof(TEntity).Name);
			set => _name = value;
		}

		public QueryBuilder<TEntity> Root { get; set; }

		public QueryBuilder<TEntity, TProp> Include<TProp>(Expression<Func<TEntity, TProp>> propertyExpression)
		{
			var body = propertyExpression.Body as MemberExpression;
			if (body == null)
				throw new ArgumentException("Expression must indicate a property.");
			var propertyName = body.Member.Name;

			var field = new QueryBuilder<TEntity, TProp>
			{
				Name = propertyName,
				Root = this
			};

			_fields.Add(field);

			return field;
		}

		public string Build()
		{
			QueryBuilder<TEntity> root = Root;
			while (root?.Root != null)
			{
				root = root.Root;
			}

			return root?.ToString() ?? ToString();
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			Build(stringBuilder, 0);

			return stringBuilder.ToString();
		}

		private void Build(StringBuilder stringBuilder, int nestLevel)
		{
			const string space = "  ";
			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			nestLevel++;

			stringBuilder.Append($"{tab}{Name.Camelize()}");
			if (_fields.Any())
			{
				stringBuilder.AppendLine(" {");
				foreach (var field in _fields)
				{
					field.Build(stringBuilder, nestLevel);
				}
				stringBuilder.Append("}");
			}

			stringBuilder.AppendLine();
		}
	}

	public class QueryParameter
	{

	}

	public class QueryBuilder<TEntity, TProp> : QueryBuilder<TEntity>
	{
		private List<QueryBuilder<TProp>> _children;

		public List<QueryBuilder<TProp>> Children
		{
			get => _children ?? (_children = new List<QueryBuilder<TProp>>());
			set => _children = value;
		}

		internal QueryBuilder() { }
	}
}
