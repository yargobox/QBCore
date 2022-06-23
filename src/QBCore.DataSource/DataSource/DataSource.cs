using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Threading.Tasks;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource> :
	IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>,
	IDataSourceHost<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>,
	ITransient<TDataSource>,
	IAsyncDisposable,
	IDisposable
{
	private readonly DSDefinition _definition;
	private IServiceProvider _serviceProvider;
	private IDataContext _dataContext;
	protected DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>? _listener;
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
		_definition = (DSDefinition) StaticFactory.DataSources[typeof(TDataSource)];
		_serviceProvider = serviceProvider;
		_dataContext = dataContextProvider.GetContext(_definition.QBFactory.DatabaseContextInterface, _definition.DataContextName);

		if (_definition.ListenerFactory != null)
		{
			_listener = (DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>) _definition.ListenerFactory(_serviceProvider);
			AsyncHelper.RunSync(async () => await _listener.OnAttachAsync(this));
		}
	}

	public IDSDefinition Definition => _definition;

	public Origin Source => throw new NotImplementedException();

	public Task<TCreate> InsertAsync(TCreate document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(IReadOnlyCollection<IDSAggregation> aggregations, SoftDel mode = SoftDel.Actual, IReadOnlyCollection<IDSCondition>? conditions = null, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<long> CountAsync(SoftDel mode = SoftDel.Actual, IReadOnlyCollection<IDSCondition>? conditions = null, DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
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

	public Task<TUpdate> UpdateAsync(TKey id, TUpdate document, IReadOnlyCollection<string>? modifiedFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TUpdate> UpdateAsync(TUpdate document, IReadOnlyCollection<IDSCondition> conditions, IReadOnlyCollection<string>? modifiedFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
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

	public Task RestoreAsync(TKey id, TRestore document, DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task RestoreAsync(TRestore document, IReadOnlyCollection<IDSCondition> conditions, DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}
}