using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBDeleteBuilder<TDoc, TDto> : QBCommonBuilder<TDoc, TDto>, IQBMongoDeleteBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public QBDeleteBuilder() { }
	public QBDeleteBuilder(QBDeleteBuilder<TDoc, TDto> other) : base(other) { }
	public QBDeleteBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		if (other.Containers.Count > 0)
		{
			var c = other.Containers.First();
			if (c.DocumentType != typeof(TDoc) || c.ContainerType != ContainerTypes.Table)
			{
				throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
			}

			var deId = DocumentInfo.IdField
				?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");

			Delete(c.DBSideName).Condition(deId, FO.In, deId.Name);
		}
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Delete query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");

		Delete().Condition(deId, FO.In, deId.Name);

		return this;
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Delete)
		{
			throw new InvalidOperationException($"Incompatible configuration of delete query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TDto> Delete(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Delete);
	}
	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.Delete(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Delete);
		return this;
	}

	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}