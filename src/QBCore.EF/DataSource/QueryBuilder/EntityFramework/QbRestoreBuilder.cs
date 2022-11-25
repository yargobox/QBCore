using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

internal sealed class QbRestoreBuilder<TDoc, TDto> : QbCommonBuilder<TDoc, TDto>, IQbEfRestoreBuilder<TDoc, TDto>
	where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public QbRestoreBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
		var deDeleted = DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public QbRestoreBuilder(QbRestoreBuilder<TDoc, TDto> other) : base(other) { }
	public QbRestoreBuilder(IQBBuilder other)
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
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
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
	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQbEfRestoreBuilder<TDoc, TDto> IQbEfRestoreBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}