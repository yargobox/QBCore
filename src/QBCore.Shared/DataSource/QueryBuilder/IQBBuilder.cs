/* using System.Data;
using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }

	DSDocumentInfo DocumentInfo { get; }
	DSDocumentInfo? ProjectionInfo { get; }

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
}

public interface IQBInsertBuilder<TDocument, TCreate> : IQBBuilder<TDocument, TCreate>
{
	Func<IDSIdGenerator>? CustomIdGenerator { get; set; }

	IQBInsertBuilder<TDocument, TCreate> InsertTo(string tableName);
	IQBInsertBuilder<TDocument, TCreate> ExecProcedure(string tableName);
	IQBInsertBuilder<TDocument, TCreate> AutoBindParameters();
	IQBInsertBuilder<TDocument, TCreate> BindParameter(Expression<Func<TCreate, object?>> field, ParameterDirection direction, bool isErrorCode = false);
	IQBInsertBuilder<TDocument, TCreate> BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false);
	IQBInsertBuilder<TDocument, TCreate> BindReturnValueToErrorCode();
	IQBInsertBuilder<TDocument, TCreate> BindParameterToErrorMessage(string name);
}



public interface IQBUpdateBuilder<TDocument, TUpdate> : IQBBuilder<TDocument, TUpdate>
{
}

public interface IQBDeleteBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}

public interface IQBSoftDelBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}

public interface IQBRestoreBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
} */