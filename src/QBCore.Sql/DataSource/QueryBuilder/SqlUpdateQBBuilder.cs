using System.Linq.Expressions;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlUpdateQBBuilder<TDoc, TUpdate> : SqlCommonQBBuilder<TDoc, TUpdate>, ISqlUpdateQBBuilder<TDoc, TUpdate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public SqlUpdateQBBuilder()
	{
		if (DocInfo.IdField == null) throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
	}
	public SqlUpdateQBBuilder(SqlUpdateQBBuilder<TDoc, TUpdate> other) : base(other) { }
	public SqlUpdateQBBuilder(IQBBuilder other) : this()
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TUpdate).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || (top.ContainerType != ContainerTypes.Table && top.ContainerType != ContainerTypes.View))
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TUpdate).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		AutoBuild(top.DBSideName);
	}

	protected override void OnPrepare()
	{
		var top = Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of update query builder '{typeof(TUpdate).ToPretty()}'.");
		}

		if (Conditions.Count == 0)
		{
			throw EX.QueryBuilder.Make.QueryBuilderMustHaveAtLeastOneCondition(DataLayer.Name, QueryBuilderType.ToString());
		}
	}

	public override QBBuilder<TDoc, TUpdate> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Update query builder '{typeof(TUpdate).ToPretty()}' has already been initialized.");
		}

		Update(tableName)
			.Condition(DocInfo.IdField!, FO.Equal, "@id");

		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TUpdate> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Begin()
	{
		Begin();
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.End()
	{
		End();
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.And()
	{
		And();
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Or()
	{
		Or();
		return this;
	}
}