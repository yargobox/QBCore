using QBCore.Configuration;
using QBCore.ObjectFactory;
using QBCore.Threading.Tasks;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TDataSource> :
	IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>,
	IDataSourceHost<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>,
	ITransient<TDataSource>,
	IAsyncDisposable,
	IDisposable
{
	private IServiceProvider _serviceProvider;
	private IDataContext _dataContext;
	protected DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>? _nativeListener;
	private object? _syncRoot;

	public object SyncRoot
	{
		get
		{
			if (_syncRoot == null) System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot!, new Object(), null!);
			return _syncRoot;
		}
	}

	public DataSource(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider)
	{
		_serviceProvider = serviceProvider;
		_dataContext = dataContextProvider.GetContext(_dataSourceDesc.DatabaseContextInterfaceType, _dataSourceDesc.DataContextName);

		if (_createNativeListener != null)
		{
			_nativeListener = _createNativeListener(_serviceProvider);
			AsyncHelper.RunSync(async () => await _nativeListener.OnAttachAsync(this));
		}
	}

	public IDataSourceDesc DataSourceDesc => _dataSourceDesc;

	public Origin Source => throw new NotImplementedException();

	public Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(IReadOnlyCollection<IDSAggregation> aggregations, SoftDel mode = SoftDel.Actual, IReadOnlyCollection<IDSCondition>? conditions = null, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<long> CountAsync(SoftDel mode = SoftDel.Actual, IReadOnlyCollection<IDSCondition>? conditions = null, DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(TKey id, TDelete document, DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(TDelete document, IReadOnlyCollection<IDSCondition> conditions, DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TCreate> InsertAsync(TCreate document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task RestoreAsync(TKey id, TDelete document, DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task RestoreAsync(TDelete document, IReadOnlyCollection<IDSCondition> conditions, DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TSelect> SelectAsync(TKey id, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<TSelect> SelectAsync(SoftDel mode = SoftDel.Actual, IReadOnlyCollection<IDSCondition>? conditions = null, IReadOnlyCollection<IDSSortOrder>? sortOrders = null, long? skip = null, int? take = null, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<bool> TestDeleteAsync(TKey? id = default, TDelete? document = default, DataSourceTestDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<bool> TestInsertAsync(DataSourceTestInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<bool> TestRestoreAsync(TKey? id = default, TDelete? document = default, DataSourceTestRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<bool> TestUpdateAsync(TKey? id = default, DataSourceTestUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TUpdate> UpdateAsync(TKey id, TUpdate document, IReadOnlyCollection<string>? modifiedFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TUpdate> UpdateAsync(TUpdate document, IReadOnlyCollection<IDSCondition> conditions, IReadOnlyCollection<string>? modifiedFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}
}