using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBRestoreBuilder<TDoc, TDto> : QBCommonBuilder<TDoc, TDto>, IQBMongoRestoreBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public QBRestoreBuilder() { }
	public QBRestoreBuilder(QBRestoreBuilder<TDoc, TDto> other) : base(other) { }
	public QBRestoreBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		if (other.Containers.Count > 0)
		{
			var c = other.Containers.First();
			if (c.DocumentType != typeof(TDoc) || c.ContainerType != ContainerTypes.Table)
			{
				throw new InvalidOperationException($"Could not make restore query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
			}

			var deId = DocumentInfo.IdField
				?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
			var deDeleted = DocumentInfo.DateDeletedField
				?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
			if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
				throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");

			Update(c.DBSideName).Condition(deId, FO.In, deId.Name).Condition(deDeleted, null, FO.NotEqual);
		}
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Restore query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
		var deDeleted = DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");

		Update().Condition(deId, FO.In, deId.Name).Condition(deDeleted, null, FO.NotEqual);

		return this;
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
	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQBMongoRestoreBuilder<TDoc, TDto> IQBMongoRestoreBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}