using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

internal sealed class QbDeleteBuilder<TDoc, TDto> : QbCommonBuilder<TDoc, TDto>, IQbEfDeleteBuilder<TDoc, TDto>
	where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public QbDeleteBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");
	}
	public QbDeleteBuilder(QbDeleteBuilder<TDoc, TDto> other) : base(other) { }
	public QbDeleteBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Delete query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		AutoBuildSetup(null);
		return this;
	}
	private void AutoBuildSetup(string? tableName)
	{
		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id field.");

		Delete(tableName).Condition(deId, FO.Equal, "id");
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
	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.Delete(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Delete);
		return this;
	}

	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.Begin()
	{
		Begin();
		return this;
	}
	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.End()
	{
		End();
		return this;
	}

	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.And()
	{
		And();
		return this;
	}
	IQbEfDeleteBuilder<TDoc, TDto> IQbEfDeleteBuilder<TDoc, TDto>.Or()
	{
		Or();
		return this;
	}
}