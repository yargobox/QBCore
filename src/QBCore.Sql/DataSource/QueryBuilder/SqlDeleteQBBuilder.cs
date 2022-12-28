using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlDeleteQBBuilder<TDoc, TRestore> : SqlCommonQBBuilder<TDoc, TRestore>, ISqlDeleteQBBuilder<TDoc, TRestore> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public SqlDeleteQBBuilder()
	{
		if (DocumentInfo.IdField == null)
		{
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		}
	}
	public SqlDeleteQBBuilder(SqlDeleteQBBuilder<TDoc, TRestore> other) : base(other) { }
	public SqlDeleteQBBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TRestore).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TRestore).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}

	public override QBBuilder<TDoc, TRestore> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Delete query builder '{typeof(TRestore).ToPretty()}' has already been initialized.");
		}

		AutoBuildSetup(null);
		return this;
	}
	private void AutoBuildSetup(string? tableName)
	{
		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");

		Delete(tableName).Condition(deId, FO.Equal, "id");
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Delete)
		{
			throw new InvalidOperationException($"Incompatible configuration of delete query builder '{typeof(TRestore).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TRestore> Delete(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Delete);
	}
	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.Delete(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Delete);
		return this;
	}

	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.Begin()
	{
		Begin();
		return this;
	}
	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.End()
	{
		End();
		return this;
	}

	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.And()
	{
		And();
		return this;
	}
	ISqlDeleteQBBuilder<TDoc, TRestore> ISqlDeleteQBBuilder<TDoc, TRestore>.Or()
	{
		Or();
		return this;
	}
}