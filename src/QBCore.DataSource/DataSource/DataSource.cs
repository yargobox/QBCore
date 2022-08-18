using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using QBCore.Configuration;
using QBCore.DataSource.Options;
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
	private readonly IServiceProvider _serviceProvider;
	private readonly IMapper _mapper;
	private readonly IDataContext _dataContext;
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
		DSInfo = StaticFactory.DataSources[typeof(TDataSource)];

		_serviceProvider = serviceProvider;
		_mapper = _serviceProvider.GetRequiredService<IMapper>();
		_dataContext = dataContextProvider.GetContext(DSInfo.QBFactory.DataLayer.DatabaseContextInterface, DSInfo.DataContextName);

		if (DSInfo.ListenerFactory != null)
		{
			_listener = (DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>) DSInfo.ListenerFactory(_serviceProvider);
			AsyncHelper.RunSync(async () => await _listener.OnAttachAsync(this));
		}
	}

	public IDSInfo DSInfo { get; }

	public async Task<TKey> InsertAsync(
		TCreate document,
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanInsert))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the insert operation.");
		}

		var getId = DSInfo.DocumentInfo.Value.IdField?.Getter
			?? throw new InvalidOperationException($"Document '{DSInfo.DocumentInfo.Value.DocumentType.ToPretty()}' does not have an id field.");

		var qb = DSInfo.QBFactory.CreateQBInsert<TDocument, TCreate>(_dataContext);
		var builder = qb.Builder;

		object? value;
		foreach (var param in builder.Parameters)
		{
			param.ResetValue();
			if (arguments != null && arguments.TryGetValue(param.Name, out value))
			{
				param.Value = value;
			}
		}

		TDocument result;
		if (typeof(TDocument) != typeof(TCreate))
		{
			result = _mapper.Map<TDocument>(document);
			result = await qb.InsertAsync(result, options, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			result = (TDocument)(object)document!;
			result = await qb.InsertAsync(result, options, cancellationToken).ConfigureAwait(false);
		}

		return (TKey) getId(result!)!;
	}

	public Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(
		IReadOnlyCollection<IDSAggregation> aggregations,
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<DSCondition<TSelect>>? conditions = null,
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<long> CountAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? conditions = null,
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceCountOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<TSelect> SelectAsync(
		TKey id,
		DataSourceSelectOptions? options = null,
		IReadOnlyDictionary<string, object?>? arguments = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? conditions = null,
		IReadOnlyList<DSSortOrder<TSelect>>? sortOrders = null,
		IReadOnlyDictionary<string, object?>? arguments = null,
		long skip = 0,
		int take = -1,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanSelect))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the select operation.");
		}

		var qb = DSInfo.QBFactory.CreateQBSelect<TDocument, TSelect>(_dataContext);
		var builder = qb.Builder;

		if (DSInfo.Options.HasFlag(DataSourceOptions.SoftDelete))
		{
			if (DSInfo.DocumentInfo.Value.DateDeletedField == null)
			{
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': option '{nameof(DSInfo.DocumentInfo.Value.DateDeletedField)}' is not set.");
			}

			if (mode == SoftDel.Actual)
			{
				builder.Condition(DSInfo.DocumentInfo.Value.DateDeletedField, null, FO.IsNull);
			}
			else if (mode == SoftDel.Deleted)
			{
				builder.Condition(DSInfo.DocumentInfo.Value.DateDeletedField, null, FO.IsNotNull);
			}
		}

		if (conditions != null)
		{
			var rootAlias = builder.Containers.First().Alias;
			foreach (var cond in conditions)
			{
				if (cond.IsByOr)
				{
					builder.Or();
				}
				
				for (int i = 0; i < cond.Parentheses; i++)
				{
					builder.Begin();
				}
				
				builder.Condition<TSelect>(rootAlias, cond.Field, cond.Value, cond.Operation);
				
				for (int i = cond.Parentheses; i < 0; i++)
				{
					builder.End();
				}
			}
		}

		if (sortOrders != null)//!!!
		{
			foreach (var sort in sortOrders)
			{
			}
		}

		object? value;
		foreach (var param in builder.Parameters)
		{
			param.ResetValue();
			if (arguments != null && arguments.TryGetValue(param.Name, out value))
			{
				param.Value = value;
			}
		}

		return await qb.SelectAsync(skip, take, options, cancellationToken).ConfigureAwait(false);
	}

	public Task<TUpdate> UpdateAsync(
		TKey id,
		TUpdate document,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyList<DSCondition<TSelect>> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(
		TKey id,
		TDelete document,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(
		TDelete document,
		IReadOnlyList<DSCondition<TSelect>> conditions,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public Task RestoreAsync(
		TKey id,
		TRestore document,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public Task RestoreAsync(
		TRestore document,
		IReadOnlyList<DSCondition<TSelect>> conditions,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}