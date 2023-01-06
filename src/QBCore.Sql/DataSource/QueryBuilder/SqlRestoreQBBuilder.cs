using System.Linq.Expressions;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlRestoreQBBuilder<TDoc, TRestore> : SqlCommonQBBuilder<TDoc, TRestore>, ISqlRestoreQBBuilder<TDoc, TRestore> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public SqlRestoreQBBuilder()
	{
		if (DocInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public SqlRestoreQBBuilder(SqlRestoreQBBuilder<TDoc, TRestore> other) : base(other) { }
	public SqlRestoreQBBuilder(IQBBuilder other) : this()
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TRestore).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || (top.ContainerType != ContainerTypes.Table && top.ContainerType != ContainerTypes.View))
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TRestore).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		AutoBuild(top.DBSideName);
	}

	protected override void OnPrepare()
	{
		var top = Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of restore query builder '{typeof(TRestore).ToPretty()}'.");
		}

		if (Conditions.Count == 0)
		{
			throw EX.QueryBuilder.Make.QueryBuilderMustHaveAtLeastOneCondition(DataLayer.Name, QueryBuilderType.ToString());
		}
	}

	public override QBBuilder<TDoc, TRestore> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Restore query builder '{typeof(TRestore).ToPretty()}' has already been initialized.");
		}

		Update(tableName)
			.Condition(DocInfo.IdField!, FO.Equal, "@id")
			.Condition(DocInfo.DateDeletedField!, null, FO.IsNotNull);

		return this;
	}
	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TRestore> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.Begin()
	{
		Begin();
		return this;
	}
	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.End()
	{
		End();
		return this;
	}

	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.And()
	{
		And();
		return this;
	}
	ISqlRestoreQBBuilder<TDoc, TRestore> ISqlRestoreQBBuilder<TDoc, TRestore>.Or()
	{
		Or();
		return this;
	}
}