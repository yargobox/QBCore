using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
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
	private readonly IServiceProvider _serviceProvider;
	private readonly IMapper _mapper;
	private readonly IDataContext _dataContext;
	protected DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>? _listener;
	private object? _syncRoot;

	public IDSInfo DSInfo { get; }
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

	public async Task<TSelect?> SelectAsync(
		TKey id,
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanSelect))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the select operation.");
		}

		if (default(TKey) == null && id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}

		var idField = DSInfo.DocumentInfo.Value.IdField
			?? throw new InvalidOperationException($"Document '{DSInfo.DocumentInfo.Value.DocumentType.ToPretty()}' does not have an id field.");

		var qb = DSInfo.QBFactory.CreateQBSelect<TDocument, TSelect>(_dataContext);
		var builder = qb.Builder;

		builder.Condition(idField, id, FO.Equal);

		object? value;
		foreach (var param in builder.Parameters)
		{
			param.ResetValue();
			if (arguments != null && arguments.TryGetValue(param.Name, out value))
			{
				param.Value = value;
			}
		}

		return await
			(await qb.SelectAsync(0, -1, options, cancellationToken).ConfigureAwait(false))
				.FirstOrDefaultAsync().ConfigureAwait(false);
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
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}

	public async Task DeleteAsync(
		TKey id,
		TDelete? document = default(TDelete?),
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceDeleteOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanDelete))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the delete operation.");
		}

		if (DSInfo.Options.HasFlag(DataSourceOptions.SoftDelete))
		{
			var qb = DSInfo.QBFactory.CreateQBSoftDel<TDocument, TDelete>(_dataContext);
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

			await qb.DeleteAsync(id!, document, options, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			var qb = DSInfo.QBFactory.CreateQBDelete<TDocument, TDelete>(_dataContext);
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

			await qb.DeleteAsync(id!, document, options, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task RestoreAsync(
		TKey id,
		TRestore? document = default(TRestore?),
		IReadOnlyDictionary<string, object?>? arguments = null,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanRestore | DataSourceOptions.SoftDelete))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the restore operation.");
		}

		var qb = DSInfo.QBFactory.CreateQBRestore<TDocument, TRestore>(_dataContext);
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

		await qb.RestoreAsync(id!, document, options, cancellationToken).ConfigureAwait(false);
	}
}