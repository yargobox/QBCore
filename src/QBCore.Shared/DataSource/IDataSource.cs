using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDataSource : IOriginal
{
	IDSDefinition Definition { get; }
}

public interface IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> : IDataSource
{
	Task<long> CountAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<IDSCondition>? conditions = null,
		DataSourceCountOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<TCreate> InsertAsync(
		TCreate document,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<TSelect> SelectAsync(
		TKey id,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<IDSAsyncCursor<TSelect>> SelectAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<IDSCondition>? conditions = null,
		IReadOnlyList<IDSSortOrder>? sortOrders = null,
		long skip = 0,
		int take = -1,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(
		IReadOnlyCollection<IDSAggregation> aggregations,
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<IDSCondition>? conditions = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<TUpdate> UpdateAsync(
		TKey id,
		TUpdate document,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyCollection<IDSCondition> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task DeleteAsync(
		TKey id,
		TDelete document,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task DeleteAsync(
		TDelete document,
		IReadOnlyCollection<IDSCondition> conditions,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task RestoreAsync(
		TKey id,
		TRestore document,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task RestoreAsync(
		TRestore document,
		IReadOnlyCollection<IDSCondition> conditions,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}