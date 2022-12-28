using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlUpdateQBBuilder<TDoc, TUpdate> : SqlCommonQBBuilder<TDoc, TUpdate>, ISqlUpdateQBBuilder<TDoc, TUpdate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public SqlUpdateQBBuilder()
	{
		if (DocumentInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
	}
	public SqlUpdateQBBuilder(SqlUpdateQBBuilder<TDoc, TUpdate> other) : base(other) { }
	public SqlUpdateQBBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TUpdate).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make update query builder '{typeof(TDoc).ToPretty()}, {typeof(TUpdate).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		AutoBuildSetup(container.DBSideName);
	}

	public override QBBuilder<TDoc, TUpdate> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Update query builder '{typeof(TUpdate).ToPretty()}' has already been initialized.");
		}

		AutoBuildSetup(null);
		return this;
	}
	private void AutoBuildSetup(string? tableName)
	{
		var deId = DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");

		Update(tableName).Condition(deId, FO.Equal, "id");
	}

	protected override void OnNormalize()
	{
		base.OnNormalize();

		if (Containers.First().ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of update query builder '{typeof(TUpdate).ToPretty()}'.");
		}
	}

	public override QBBuilder<TDoc, TUpdate> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Begin()
	{
		Begin();
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.End()
	{
		End();
		return this;
	}

	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.And()
	{
		And();
		return this;
	}
	ISqlUpdateQBBuilder<TDoc, TUpdate> ISqlUpdateQBBuilder<TDoc, TUpdate>.Or()
	{
		Or();
		return this;
	}
}