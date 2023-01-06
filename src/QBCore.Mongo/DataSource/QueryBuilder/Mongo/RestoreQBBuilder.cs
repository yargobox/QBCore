using System.Linq.Expressions;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class RestoreQBBuilder<TDoc, TRestore> : CommonQBBuilder<TDoc, TRestore>, IMongoRestoreQBBuilder<TDoc, TRestore>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public RestoreQBBuilder()
	{
		if (DocInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public RestoreQBBuilder(RestoreQBBuilder<TDoc, TRestore> other) : base(other) { }
	public RestoreQBBuilder(IQBBuilder other) : this()
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TRestore).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || top.ContainerType != ContainerTypes.Table)
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
	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TRestore> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.Begin()
	{
		Begin();
		return this;
	}
	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.End()
	{
		End();
		return this;
	}

	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.And()
	{
		And();
		return this;
	}
	IMongoRestoreQBBuilder<TDoc, TRestore> IMongoRestoreQBBuilder<TDoc, TRestore>.Or()
	{
		Or();
		return this;
	}
}