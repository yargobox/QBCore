using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBUpdateBuilder<TDoc, TDto> : QBCommonBuilder<TDoc, TDto>, IQBMongoUpdateBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public QBUpdateBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
	}
	public QBUpdateBuilder(QBUpdateBuilder<TDoc, TDto> other) : base(other) { }
	public QBUpdateBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Update query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		AutoBuildSetup(null);
		return this;
	}
	private void AutoBuildSetup(string? tableName)
	{
		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");

		Update(tableName).Condition(deId, FO.Equal, "id");
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of update query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TDto> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQBMongoUpdateBuilder<TDoc, TDto> IQBMongoUpdateBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}