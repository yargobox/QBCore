using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public static class ExtensionsForQBBuilder
{
	public static void AddCondition<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string leftTarget, string leftField, string rightTarget, string rightField, ConditionOperations operation)
	{
		if (leftTarget == rightTarget && leftField == rightField) throw new InvalidOperationException($"QB: Ambiguous condition on {typeof(TDocument).ToPretty()}.");

		var conditions = ((QBBuilder<TDocument, TProjection>)qb).Conditions;

		conditions.Add(new BuilderCondition(
			LeftTarget: leftTarget,
			LeftField: leftField,
			RightTarget: rightTarget,
			RightField: rightField,
			ConstValue: null,
			Operation: operation
		));
	}

	public static void AddCondition<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string leftTarget, string leftField, object? constValue, ConditionOperations operation)
	{
		var conditions = ((QBBuilder<TDocument, TProjection>)qb).Conditions;

		conditions.Add(new BuilderCondition(
			LeftTarget: leftTarget,
			LeftField: leftField,
			RightTarget: null,
			RightField: null,
			ConstValue: constValue,
			Operation: operation
		));
	}

	public static void AddCondition<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string leftTarget, Expression<Func<TDocument, object?>> leftFieldSelector, string rightTarget, Expression<Func<TProjection, object?>> rightFieldSelector, ConditionOperations operation)
	{
		var leftField = GetMemberName(leftFieldSelector);
		var rightField = GetMemberName(rightFieldSelector);

		if (leftTarget == rightTarget && leftField == rightField) throw new InvalidOperationException($"QB: Ambiguous condition on {typeof(TDocument).ToPretty()}.");

		var conditions = ((QBBuilder<TDocument, TProjection>)qb).Conditions;

		conditions.Add(new BuilderCondition(
			LeftTarget: leftTarget,
			LeftField: leftField,
			RightTarget: rightTarget,
			RightField: rightField,
			ConstValue: null,
			Operation: operation
		));
	}

	public static void AddCondition<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string leftTarget, Expression<Func<TDocument, object?>> leftFieldSelector, object? constValue, ConditionOperations operation)
	{
		var leftField = GetMemberName(leftFieldSelector);
		var conditions = ((QBBuilder<TDocument, TProjection>)qb).Conditions;

		conditions.Add(new BuilderCondition(
			LeftTarget: leftTarget,
			LeftField: leftField,
			RightTarget: null,
			RightField: null,
			ConstValue: constValue,
			Operation: operation
		));
	}

	public static void AddCondition<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string leftTarget, Expression<Func<TProjection, object?>> leftFieldSelector, object? constValue, ConditionOperations operation)
	{
		var leftField = GetMemberName(leftFieldSelector);
		var conditions = ((QBBuilder<TDocument, TProjection>)qb).Conditions;

		conditions.Add(new BuilderCondition(
			LeftTarget: leftTarget,
			LeftField: leftField,
			RightTarget: null,
			RightField: null,
			ConstValue: constValue,
			Operation: operation
		));
	}

	public static void AddOrder<TDocument, TProjection>(this ISelectQueryBuilder<TDocument, TProjection> qb, Expression<Func<TDocument, object?>> field, bool descending = false)
	{
		var fieldName = GetMemberName(field);
		var sortOrders = ((QBBuilder<TDocument, TProjection>)qb).SortOrders;

		if (sortOrders.Any(x => x.Field == fieldName))
			throw new InvalidOperationException($"QB: 'ORDER BY {fieldName}' has already been added before.");

		sortOrders.Add(new BuilderSortOrder(
			Field: fieldName,
			Descending: descending
		));
	}

	public static void AddParameter<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string parameterName, string targetName, string targetParameter, Type underlyingType, string dbtype, bool isNullable, System.Data.ParameterDirection direction)
	{
		var parameters = ((QBBuilder<TDocument, TProjection>)qb).Parameters;
		
		if (parameters.Any(x => x.Name == parameterName || (x.TargetName == targetName && x.TargetParameter == targetParameter)))
			throw new InvalidOperationException($"QB: Parameter {parameterName}' has already been added before.");

		parameters.Add(new BuilderParameter(
			Name: parameterName,
			TargetName: targetName,
			TargetParameter: targetParameter,
			UnderlyingType: underlyingType,
			DbType: dbtype,
			IsNullable: isNullable,
			Direction: direction
		));
	}

	public static void SelectFromFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string functionName)
	{
		qb.AddContainer(functionName, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.Select);
	}

	public static void SelectFromFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string functionName)
	{
		qb.AddContainer(alias, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.Select);
	}

	public static void ExecProcedure<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string procedureName)
	{
		qb.AddContainer(procedureName, procedureName, BuilderContainerTypes.Procedure, BuilderContainerOperations.Select);
	}

	public static void SelectFromTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select);
	}

	public static void SelectFromTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select);
	}

	public static void SelectFromView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Select);
	}

	public static void SelectFromView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Select);
	}

	public static void JoinFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string functionName)
	{
		qb.AddContainer(functionName, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.Join);
	}

	public static void JoinFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string functionName)
	{
		qb.AddContainer(alias, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.Join);
	}

	public static void JoinTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);
	}

	public static void JoinTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);
	}

	public static void JoinView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Join);
	}

	public static void JoinView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Join);
	}

	public static void LeftJoinFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string functionName)
	{
		qb.AddContainer(functionName, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.LeftJoin);
	}

	public static void LeftJoinFunction<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string functionName)
	{
		qb.AddContainer(alias, functionName, BuilderContainerTypes.Function, BuilderContainerOperations.LeftJoin);
	}

	public static void LeftJoinTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);
	}

	public static void LeftJoinTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);
	}

	public static void LeftJoinView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.LeftJoin);
	}

	public static void LeftJoinView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.LeftJoin);
	}

	public static void UpdateTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Update);
	}

	public static void UpdateTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Update);
	}

	public static void UpdateView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Update);
	}

	public static void UpdateView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Update);
	}

	public static void DeleteFromTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Delete);
	}

	public static void DeleteFromTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Delete);
	}

	public static void DeleteFromView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Delete);
	}

	public static void DeleteFromView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Delete);
	}

	public static void InsertToTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string tableName)
	{
		qb.AddContainer(tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Insert);
	}

	public static void InsertToTable<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string tableName)
	{
		qb.AddContainer(alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Insert);
	}

	public static void InsertToView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string viewName)
	{
		qb.AddContainer(viewName, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Insert);
	}

	public static void InsertToView<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string viewName)
	{
		qb.AddContainer(alias, viewName, BuilderContainerTypes.View, BuilderContainerOperations.Insert);
	}


	private static string GetMemberName(Expression expression) => ((expression as LambdaExpression)?.Body as MemberExpression)?.Member.Name ?? throw new ArgumentException("It's not a member expression");

	private static void AddContainer<TDocument, TProjection>(this IQBBuilder<TDocument, TProjection> qb, string alias, string name, BuilderContainerTypes containerType, BuilderContainerOperations containerOperation)
	{
		var containers = ((QBBuilder<TDocument, TProjection>)qb).Containers;

		if ((containerOperation & BuilderContainerOperations.MainMask) != 0)
		{
			if (containers.Any(x => (x.ContainerOperation & BuilderContainerOperations.MainMask) != 0 || x.Alias == alias))
				throw new InvalidOperationException($"QB: '{alias}' has already been added or another initial container has been added before.");
		}
		else if (containers.Any(x => x.Alias == alias))
		{
			throw new InvalidOperationException($"QB: '{alias}' has been added before.");
		}

		containers.Add(new BuilderContainer(
			Alias: alias,
			Name: name,
			ContainerType: containerType,
			ContainerOperation: containerOperation
		));
	}
}