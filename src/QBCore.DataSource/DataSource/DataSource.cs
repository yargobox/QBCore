using System.Runtime.CompilerServices;
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
		Definition = StaticFactory.DataSources[typeof(TDataSource)];
		_serviceProvider = serviceProvider;
		_dataContext = dataContextProvider.GetContext(Definition.QBFactory.DatabaseContextInterface, Definition.DataContextName);

		if (Definition.ListenerFactory != null)
		{
			_listener = (DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>) Definition.ListenerFactory(_serviceProvider);
			AsyncHelper.RunSync(async () => await _listener.OnAttachAsync(this));
		}
	}

	public IDSDefinition Definition { get; }

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

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(SoftDel mode = SoftDel.Actual, IReadOnlyList<IDSCondition>? conditions = null, IReadOnlyList<IDSSortOrder>? sortOrders = null, long skip = 0, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var queryBuilder = Definition.QBFactory.CreateQBSelect<TDocument, TSelect>(_dataContext);
		var builder = queryBuilder.SelectBuilder;

		if (Definition.Options.HasFlag(DataSourceOptions.SoftDelete))
		{
			if (builder.DateDeleteField == null)
			{
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': option '{nameof(builder.DateDeleteField)}' is not set.");
			}

			if (mode == SoftDel.Actual)
			{
				builder.Condition(builder.DateDeleteField, null, FO.IsNull);
			}
			else if (mode == SoftDel.Deleted)
			{
				builder.Condition(builder.DateDeleteField, null, FO.IsNotNull);
			}
		}

		if (conditions != null)
		{
			foreach (var cond in conditions)
			{
			}
		}

		if (sortOrders != null)
		{
			foreach (var sort in sortOrders)
			{
			}
		}

		return await queryBuilder.SelectAsync(skip, take, options, cancellationToken).ConfigureAwait(false);
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