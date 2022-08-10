using System.Data;
using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }

	IReadOnlyList<QBContainer> Containers { get; }
	IReadOnlyList<QBCondition> Connects { get; }
	IReadOnlyList<QBCondition> Conditions { get; }
	IReadOnlyList<QBField> Fields { get; }
	IReadOnlyList<QBParameter> Parameters { get; }
	IReadOnlyList<QBSortOrder> SortOrders { get; }
	IReadOnlyList<QBAggregation> Aggregations { get; }

	bool IsNormalized { get; }

	void Normalize();
}

public interface IQBBuilder<TDocument, TProjection> : IQBBuilder
{
	Expression<Func<TDocument, object?>>? IdField { get; set; }
	Func<TDocument, object?>? IdGetter { get; }
	Action<TDocument, object?>? IdSetter { get; }
}

public interface IQBInsertBuilder<TDocument, TCreate> : IQBBuilder<TDocument, TCreate>
{
	Func<IDSIdGenerator>? CustomIdGenerator { get; set; }
	Expression<Func<TDocument, object?>>? DateCreateField { get; set; }
	Func<TDocument, object?>? DateCreateGetter { get; }
	Action<TDocument, object?>? DateCreateSetter { get; }
	Expression<Func<TDocument, object?>>? DateModifyField { get; set; }
	Func<TDocument, object?>? DateModifyGetter { get; }
	Action<TDocument, object?>? DateModifySetter { get; }

	IQBInsertBuilder<TDocument, TCreate> InsertTo(string tableName);
	IQBInsertBuilder<TDocument, TCreate> ExecProcedure(string tableName);
	IQBInsertBuilder<TDocument, TCreate> AutoBindParameters();
	IQBInsertBuilder<TDocument, TCreate> BindParameter(Expression<Func<TCreate, object?>> field, ParameterDirection direction, bool isErrorCode = false);
	IQBInsertBuilder<TDocument, TCreate> BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false);
	IQBInsertBuilder<TDocument, TCreate> BindReturnValueToErrorCode();
	IQBInsertBuilder<TDocument, TCreate> BindParameterToErrorMessage(string name);
}

public interface IQBSelectBuilder<TDocument, TSelect> : IQBBuilder<TDocument, TSelect>
{
	Expression<Func<TSelect, object?>>? DateDeleteField { get; set; }

	IQBSelectBuilder<TDocument, TSelect> SelectFrom(string tableName);
	IQBSelectBuilder<TDocument, TSelect> SelectFrom(string alias, string tableName);

	IQBSelectBuilder<TDocument, TSelect> LeftJoin<TRef>(string tableName);
	IQBSelectBuilder<TDocument, TSelect> LeftJoin<TRef>(string alias, string tableName);

	IQBSelectBuilder<TDocument, TSelect> Join<TRef>(string tableName);
	IQBSelectBuilder<TDocument, TSelect> Join<TRef>(string alias, string tableName);

	IQBSelectBuilder<TDocument, TSelect> CrossJoin<TRef>(string tableName);
	IQBSelectBuilder<TDocument, TSelect> CrossJoin<TRef>(string alias, string tableName);

	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBSelectBuilder<TDocument, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBSelectBuilder<TDocument, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBSelectBuilder<TDocument, TSelect> Condition(Expression<Func<TDocument, object?>> field, object? constValue, FO operation);
	IQBSelectBuilder<TDocument, TSelect> Condition(Expression<Func<TDocument, object?>> field, FO operation, string paramName);

	IQBSelectBuilder<TDocument, TSelect> Begin();
	IQBSelectBuilder<TDocument, TSelect> End();
	
	IQBSelectBuilder<TDocument, TSelect> And();
	IQBSelectBuilder<TDocument, TSelect> Or();

	IQBSelectBuilder<TDocument, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBSelectBuilder<TDocument, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IQBSelectBuilder<TDocument, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IQBSelectBuilder<TDocument, TSelect> Optional(Expression<Func<TSelect, object?>> field);
}

public interface IQBUpdateBuilder<TDocument, TUpdate> : IQBBuilder<TDocument, TUpdate>
{
	Expression<Func<TDocument, object?>>? DateUpdateField { get; set; }
	Expression<Func<TDocument, object?>>? DateModifyField { get; set; }
}

public interface IQBDeleteBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}

public interface IQBSoftDelBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
	Expression<Func<TDocument, object?>>? DateDeleteField { get; set; }
}

public interface IQBRestoreBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
	Expression<Func<TDocument, object?>>? DateDeleteField { get; set; }
}