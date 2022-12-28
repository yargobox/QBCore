using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class QBRestoreBuilder<TDoc, TDto> : QBCommonBuilder<TDoc, TDto>, IQBEfCoreRestoreBuilder<TDoc, TDto>
	where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public QBRestoreBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public QBRestoreBuilder(QBRestoreBuilder<TDoc, TDto> other) : base(other) { }
	public QBRestoreBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Restore query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		AutoBuildSetup(null);
		return this;
	}
	private void AutoBuildSetup(string? tableName)
	{
		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");

		Update(tableName).Condition(deId, FO.Equal, "id").Condition(deDeleted, null, FO.NotEqual);
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of restore query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TDto> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQBEfCoreRestoreBuilder<TDoc, TDto> IQBEfCoreRestoreBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}