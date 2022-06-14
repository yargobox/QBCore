using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDataSource : IOriginal
{
	IDataSourceDesc DataSourceDesc { get; }
}

public interface IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete> : IDataSource
{
	Task<long> CountAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<IDSCondition>? conditions = null,
		DataSourceCountOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<bool> TestInsertAsync(
		DataSourceTestInsertOptions? options = null,
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
	IAsyncEnumerable<TSelect> SelectAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<IDSCondition>? conditions = null,
		IReadOnlyCollection<IDSSortOrder>? sortOrders = null,
		long? skip = null,
		int? take = null,
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
	Task<bool> TestUpdateAsync(
		TKey? id = default(TKey?),
		DataSourceTestUpdateOptions? options = null,
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
	Task<bool> TestDeleteAsync(
		TKey? id = default(TKey?),
		TDelete? document = default(TDelete?),
		DataSourceTestDeleteOptions? options = null,
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
	Task<bool> TestRestoreAsync(
		TKey? id = default(TKey?),
		TDelete? document = default(TDelete?),
		DataSourceTestRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task RestoreAsync(
		TKey id,
		TDelete document,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task RestoreAsync(
		TDelete document,
		IReadOnlyCollection<IDSCondition> conditions,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}