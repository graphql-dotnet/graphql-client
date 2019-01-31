using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public class QueryBuilder : IQueryBuilderInternal
	{
		private readonly List<IQueryBuilder> _fields = new List<IQueryBuilder>();
		private readonly List<QueryParameterUsage> _parameterUsages = new List<QueryParameterUsage>();
		private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
		private IQueryBuilderInternal _currentField;
		private IQueryBuilderInternal _parent;

		public string Name { get; set; }

		IQueryBuilderInternal IQueryBuilderInternal.Parent
		{
			get => _parent;
			set => _parent = value;
		}

		IQueryBuilderInternal IQueryBuilderInternal.CurrentField
		{
			get => _currentField;
			set => _currentField = value;
		}

		public QueryBuilder()
		{
			_currentField = this;
		}

		public static IQueryBuilder<TEntity, object, object> New<TEntity>()
		{
			return new QueryBuilder<TEntity, object, object>();
		}

		protected QueryBuilder(QueryBuilder source)
		{
			var asInternal = (IQueryBuilderInternal)source;
			_fields = new List<IQueryBuilder>(source._fields);
			_parameters.AddRange(source._parameters);
			_parameterUsages.AddRange(source._parameterUsages);
			_currentField = ReferenceEquals(asInternal.CurrentField, asInternal)
				? this
				: asInternal.CurrentField;
		}

		public string Build()
		{
			var root = _parent;
			while (root?.Parent != null)
			{
				root = root.Parent;
			}

			return root?.ToString() ?? ToString();
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append($"query {Name}");
			if (_parameters.Any())
				stringBuilder.Append($"({string.Join(", ", _parameters)})");

			stringBuilder.AppendLine(" {");

			Build(stringBuilder, 1);

			stringBuilder.AppendLine("}");

			return stringBuilder.ToString();
		}

		private void Build(StringBuilder stringBuilder, int nestLevel)
		{
			const string space = "  ";

			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			nestLevel++;

			stringBuilder.Append($"{tab}{Name.Camelize()}");

			if (_parameterUsages.Any())
				stringBuilder.Append($"({string.Join(", ", _parameterUsages)})");

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

		IQueryBuilderInternal IQueryBuilderInternal.TryAddField(IQueryBuilder field)
		{
			// TODO maybe replace with dictionary
			var found = _fields.FirstOrDefault(f => f.Name == field.Name);
			if (found == null)
			{
				_fields.Add(field);
				found = field;
			}

			return (IQueryBuilderInternal) found;
		}

		void IQueryBuilderInternal.AddParameter(QueryParameter parameter)
		{
			_parameters.Add(parameter);
		}

		void IQueryBuilderInternal.AddParameter(QueryParameterUsage parameter)
		{
			_parameterUsages.Add(parameter);
		}
	}

	internal class QueryBuilder<TEntity, TProp, TParams> : QueryBuilder, IQueryBuilder<TEntity, TProp, TParams>
	{
		public QueryBuilder()
		{
			Name = typeof(TEntity).Name;
		}

		public QueryBuilder(QueryBuilder source)
			: base(source)
		{
			Name = typeof(TEntity).Name;
		}
	}
}
