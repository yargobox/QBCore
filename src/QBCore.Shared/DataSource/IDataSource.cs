using QBCore.DataSource.Options;

namespace QBCore.DataSource;

public interface IDataSource
{
	DSKeyName OKeyName { get; }
	IDSInfo DSInfo { get; }

	void Init(DSKeyName? keyName = null, bool shared = true);
}

public interface IDataSource<TKey, TCreate, TSelect, TUpdate, TDelete, TRestore> : IDataSource
{
	Task<TKey> InsertAsync(
		TCreate document,
		IDictionary<string, object?>? parameters = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(
		IReadOnlyCollection<IDSAggregation> aggregations,
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<DSCondition<TSelect>>? conditions = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<long> CountAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? conditions = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceCountOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<TSelect?> SelectAsync(
		TKey id,
		IDictionary<string, object?>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task<IDSAsyncCursor<TSelect>> SelectAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? conditions = null,
		IReadOnlyList<DSSortOrder<TSelect>>? sortOrders = null,
		IDictionary<string, object?>? parameters = null,
		long skip = 0,
		int take = -1,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task UpdateAsync(
		TKey id,
		TUpdate document,
		IReadOnlySet<string>? modifiedFieldNames = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task DeleteAsync(
		TKey id,
		TDelete? document = default(TDelete?),
		IDictionary<string, object?>? parameters = null,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
	Task RestoreAsync(
		TKey id,
		TRestore? document = default(TRestore?),
		IDictionary<string, object?>? parameters = null,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface IDataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
	: IDataSource<TKey, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null);
}