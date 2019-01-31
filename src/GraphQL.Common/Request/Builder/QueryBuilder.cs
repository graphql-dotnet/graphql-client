using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace GraphQL.Common.Request.Builder
{
	public class QueryBuilder : IQueryBuilderInternal
	{
		private readonly string _queryType;
		private readonly List<IQueryBuilderInternal> _fields = new List<IQueryBuilderInternal>();
		private readonly List<QueryParameterUsage> _parameterUsages = new List<QueryParameterUsage>();
		private readonly List<QueryParameter> _parameters = new List<QueryParameter>();
		private IQueryBuilderInternal _currentField;
		private string _operationName;
		private string _name;

		string IQueryBuilderInternal.OperationName
		{
			get => _operationName;
			set => _operationName = value;
		}

		string IQueryBuilderInternal.Name
		{
			get => _name;
			set => _name = value;
		}

		IQueryBuilderInternal IQueryBuilderInternal.CurrentField
		{
			get => _currentField;
			set => _currentField = value;
		}

		internal QueryBuilder()
		{
			_currentField = this;
		}
		internal QueryBuilder(string queryType)
			: this()
		{
			_queryType = queryType;
		}

		public static IQueryBuilder<TEntity, object, object> Query<TEntity>(string operationName = null)
		{
			var queryBuilder = new QueryBuilder<TEntity, object, object>("query");
			var asInternal = (IQueryBuilderInternal) queryBuilder;
			asInternal.OperationName = operationName ?? typeof(TEntity).Name;
			asInternal.Name = typeof(TEntity).Name;
			return queryBuilder;
		}

		public static IQueryBuilder<TEntity, object, object> Mutation<TEntity>(string operationName = null)
		{
			var queryBuilder = new QueryBuilder<TEntity, object, object>("mutation");
			var asInternal = (IQueryBuilderInternal)queryBuilder;
			asInternal.OperationName = operationName ?? typeof(TEntity).Name;
			asInternal.Name = typeof(TEntity).Name;
			return queryBuilder;
		}

		protected QueryBuilder(QueryBuilder source)
		{
			_queryType = source._queryType;
			_operationName = source._operationName;
			_name = source._name;
			_fields = new List<IQueryBuilderInternal>(source._fields);
			_parameters.AddRange(source._parameters);
			_parameterUsages.AddRange(source._parameterUsages);
			_currentField = ReferenceEquals(source._currentField, source)
				? this
				: source._currentField;
			
		}

		public string Build()
		{
			if (_queryType == "mutation" && !_parameters.Any())
				throw new ArgumentException("Parameters have not been set for mutation.");

			return ToString();
		}

		public override string ToString()
		{
			var asInternal = (IQueryBuilderInternal) this;
			var stringBuilder = new StringBuilder();
			stringBuilder.Append($"{_queryType} {asInternal.OperationName}");
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

			var asInternal = (IQueryBuilderInternal) this;
			var tab = string.Join(string.Empty, Enumerable.Repeat(space, nestLevel));
			nestLevel++;

			stringBuilder.Append($"{tab}{asInternal.Name.Camelize()}");

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

		IQueryBuilderInternal IQueryBuilderInternal.TryAddField(IQueryBuilderInternal field)
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
		public QueryBuilder(string queryType)
			: base(queryType)
		{
		}

		public QueryBuilder(QueryBuilder source)
			: base(source)
		{
		}
	}
}
