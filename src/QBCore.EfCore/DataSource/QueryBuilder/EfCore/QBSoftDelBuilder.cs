using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class QBSoftDelBuilder<TDoc, TDto> : QBCommonBuilder<TDoc, TDto>, IQBEfCoreSoftDelBuilder<TDoc, TDto>
	where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public QBSoftDelBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public QBSoftDelBuilder(QBSoftDelBuilder<TDoc, TDto> other) : base(other) { }
	public QBSoftDelBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make soft delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make soft delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Soft delete query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
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

		Update(tableName).Condition(deId, FO.Equal, "id").Condition(deDeleted, null, FO.Equal);
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of soft delete query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TDto> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQBEfCoreSoftDelBuilder<TDoc, TDto> IQBEfCoreSoftDelBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}