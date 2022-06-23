using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilder : IOriginal
{
	Type DatabaseContextInterface { get; }
	QueryBuilderTypes QueryBuilderType { get; }
	Type DocumentType { get; }
	Type ProjectionType { get; }
}

public interface IQueryBuilder<TDocument, TProjection> : IQueryBuilder
{
}

public interface IInsertQueryBuilder<TDocument, TCreate> : IQueryBuilder<TDocument, TCreate>
{
	Task<TCreate> InsertAsync(
		TCreate document,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface ISelectQueryBuilder<TDocument, TSelect> : IQueryBuilder<TDocument, TSelect>
{
	Task<long> CountAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);

	IAsyncEnumerable<TSelect> SelectAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		IReadOnlyCollection<QBSortOrder>? sortOrders = null,
		long? skip = null,
		int? take = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface IUpdateQueryBuilder<TDocument, TUpdate> : IQueryBuilder<TDocument, TUpdate>
{
	Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyCollection<QBCondition> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface IDeleteQueryBuilder<TDocument, TDelete> : IQueryBuilder<TDocument, TDelete>
{
}

public interface IRestoreQueryBuilder<TDocument, TRestore> : IQueryBuilder<TDocument, TRestore>
{
}