using System.Collections.Concurrent;
using System.Data;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Internals;
using QBCore.Extensions.Threading.Tasks;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource> :
	IDataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>,
	IDataSourceHost<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>,
	ITransient<TDataSource>,
	IAsyncDisposable,
	IDisposable
	where TDoc : class
{
	public DSKeyName OKeyName => _okeyName ?? throw new InvalidOperationException($"DataSource {DSInfo.Name} has not been initialized yet.");
	public IDSInfo DSInfo { get; }
	public object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;

	protected IMapper Mapper => _mapper ?? (_mapper = _serviceProvider.GetRequiredService<IMapper>());

	private readonly IServiceProvider _serviceProvider;
	private IMapper? _mapper;
	private readonly IDataContext _dataContext;
	private DSKeyName? _okeyName;
	protected List<DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>>? _listeners;
	private object? _syncRoot;
	protected ConcurrentDictionary<OKeyName, object?>? _internalObjects;

	public DataSource(IServiceProvider serviceProvider)
	{
		DSInfo = StaticFactory.DataSources[typeof(TDataSource)];

		_serviceProvider = serviceProvider;
		var dataContextProvider = (IDataContextProvider) _serviceProvider.GetRequiredService(DSInfo.QBFactory.DataLayer.DataContextProviderInterfaceType);
		_dataContext = dataContextProvider.GetDataContext(DSInfo.DataContextName);

		_listeners = DSInfo.Listeners?
			.Select(ctor => ctor(_serviceProvider))
			.Cast<DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>>()
			.ToList();
	}

	public async Task<TKey> InsertAsync(
		TCreate document,
		IDictionary<string, object?>? parameters = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanInsert))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the insert operation.");
		}

		var getId = DSInfo.DocInfo.Value.IdField?.Getter
			?? throw new InvalidOperationException($"Document '{DSInfo.DocInfo.Value.DocumentType.ToPretty()}' does not have an id data entry.");

		var qb = DSInfo.QBFactory.CreateQBInsert<TDoc, TCreate>(_dataContext);

		object? value;
		foreach (var param in qb.Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
		{
			param.ResetValue();

			if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
			{
				param.Value = value;
			}
			else if (param.ParameterName.StartsWith('='))
			{
				if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
			}
		}

		if (qb.Builder.IsDocumentMapperRequired)
		{
			options ??= new DataSourceInsertOptions();
			options.DocumentMapper ??= TDoc (TCreate dto) => Mapper.Map<TCreate, TDoc>(dto);
		}

		var result = await qb.InsertAsync(document, options, cancellationToken).ConfigureAwait(false);

		UpdateOutputParameters(parameters, qb.Builder.Parameters);

		return (TKey) result;
	}

	public IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<KeyValuePair<string, object?>>> AggregateAsync(
		IReadOnlyCollection<IDSAggregation> aggregations,
		SoftDel mode = SoftDel.Actual,
		IReadOnlyCollection<DSCondition<TSelect>>? conditions = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public Task<long> CountAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? conditions = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceCountOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public async Task<TSelect?> SelectAsync(
		TKey id,
		IDictionary<string, object?>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanSelect))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the select operation.");
		}

		if (default(TKey) is null && id is null)
		{
			throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		}

		var idField = DSInfo.DocInfo.Value.IdField
			?? throw new InvalidOperationException($"Document '{DSInfo.DocInfo.Value.DocumentType.ToPretty()}' does not have an id data entry.");

		var qb = DSInfo.QBFactory.CreateQBSelect<TDoc, TSelect>(_dataContext);
		var builder = qb.Builder;

		builder.Condition(idField, id, FO.Equal);

		object? value;
		foreach (var param in builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
		{
			param.ResetValue();

			if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
			{
				param.Value = value;
			}
			else if (param.ParameterName.StartsWith('='))
			{
				if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
				else
				{
					switch (param.ParameterName)
					{
						case "=skip": param.Value = 0; break;
						case "=take": param.Value = -1; break;
						case "=mode": param.Value = (int)2; break;
					}
				}
			}
		}

		return await
			(await qb.SelectAsync(0, -1, options, cancellationToken).ConfigureAwait(false))
				.FirstOrDefaultAsync().ConfigureAwait(false);
	}

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(
		SoftDel mode = SoftDel.Actual,
		IReadOnlyList<DSCondition<TSelect>>? filter = null,
		IReadOnlyList<DSSortOrder<TSelect>>? sort = null,
		IDictionary<string, object?>? parameters = null,
		long skip = 0,
		int take = -1,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanSelect))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the select operation.");
		}

		var qb = DSInfo.QBFactory.CreateQBSelect<TDoc, TSelect>(_dataContext);
		var builder = qb.Builder;

		if (DSInfo.Options.HasFlag(DataSourceOptions.SoftDelete))
		{
			var deDeleted = builder.DtoInfo?.DateDeletedField ?? builder.DocInfo.DateDeletedField
				?? throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': option '{nameof(DSInfo.DocInfo.Value.DateDeletedField)}' is not set.");

			if (mode == SoftDel.Actual)
			{
				builder.Condition(deDeleted, null, FO.IsNull);
			}
			else if (mode == SoftDel.Deleted)
			{
				builder.Condition(deDeleted, null, FO.IsNotNull);
			}
		}

		if (filter != null)
		{
			var rootAlias = builder.Containers.First().Alias;
			foreach (var cond in filter)
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

		if (sort != null)
		{
			foreach (var so in sort)
			{
				if (builder.SortOrders.Any(x => x.SortOrder == so.SortOrder && x.Field.ToString() == so.Field.Path))
				{
					continue;
				}

				builder.SortBy(so.Field, so.SortOrder);
			}
		}

		object? value;
		foreach (var param in builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
		{
			param.ResetValue();

			if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
			{
				param.Value = value;
			}
			else if (param.ParameterName.StartsWith('='))
			{
				if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
				else
				{
					switch (param.ParameterName)
					{
						case "=skip": param.Value = skip; break;
						case "=take": param.Value = take; break;
						case "=mode": param.Value = (int)mode; break;
					}
				}
			}
		}

		return await qb.SelectAsync(skip, take, options, cancellationToken).ConfigureAwait(false);
	}

	public async Task UpdateAsync(
		TKey id,
		TUpdate document,
		IReadOnlySet<string>? validFieldNames = null,
		IDictionary<string, object?>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanUpdate))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the update operation.");
		}

		var qb = DSInfo.QBFactory.CreateQBUpdate<TDoc, TUpdate>(_dataContext);

		object? value;
		foreach (var param in qb.Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
		{
			param.ResetValue();

			if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
			{
				param.Value = value;
			}
			else if (param.ParameterName.StartsWith('='))
			{
				if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
			}
		}

		await qb.UpdateAsync(id!, document, validFieldNames, options, cancellationToken).ConfigureAwait(false);
	
		UpdateOutputParameters(parameters, qb.Builder.Parameters);
	}

	public async Task DeleteAsync(
		TKey id,
		TDelete? document = default(TDelete?),
		IDictionary<string, object?>? parameters = null,
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
			var qb = DSInfo.QBFactory.CreateQBSoftDel<TDoc, TDelete>(_dataContext);

			object? value;
			foreach (var param in qb.Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
			{
				param.ResetValue();

				if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
				else if (param.ParameterName.StartsWith('='))
				{
					if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
					{
						param.Value = value;
					}
				}
			}

			await qb.DeleteAsync(id!, document, options, cancellationToken).ConfigureAwait(false);

			UpdateOutputParameters(parameters, qb.Builder.Parameters);
		}
		else
		{
			var qb = DSInfo.QBFactory.CreateQBDelete<TDoc, TDelete>(_dataContext);

			object? value;
			foreach (var param in qb.Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
			{
				param.ResetValue();

				if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
				else if (param.ParameterName.StartsWith('='))
				{
					if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
					{
						param.Value = value;
					}
				}
			}

			await qb.DeleteAsync(id!, document, options, cancellationToken).ConfigureAwait(false);

			UpdateOutputParameters(parameters, qb.Builder.Parameters);
		}
	}

	public async Task RestoreAsync(
		TKey id,
		TRestore? document = default(TRestore?),
		IDictionary<string, object?>? parameters = null,
		DataSourceRestoreOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		if (!DSInfo.Options.HasFlag(DataSourceOptions.CanRestore | DataSourceOptions.SoftDelete))
		{
			throw new InvalidOperationException($"DataSource {DSInfo.Name} does not support the restore operation.");
		}

		var qb = DSInfo.QBFactory.CreateQBRestore<TDoc, TRestore>(_dataContext);

		object? value;
		foreach (var param in qb.Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
		{
			param.ResetValue();

			if (parameters != null && parameters.TryGetValue(param.ParameterName, out value))
			{
				param.Value = value;
			}
			else if (param.ParameterName.StartsWith('='))
			{
				if (_dataContext.Args != null && _dataContext.Args.TryGetValue(param.ParameterName, out value))
				{
					param.Value = value;
				}
			}
		}

		await qb.RestoreAsync(id!, document, options, cancellationToken).ConfigureAwait(false);

		UpdateOutputParameters(parameters, qb.Builder.Parameters);
	}

	private static void UpdateOutputParameters(IDictionary<string, object?>? parameters, IReadOnlyList<QBParameter> qbParameters)
	{
		if (parameters != null)
		{
			foreach (var param in qbParameters.Where(x => x.Direction.HasFlag(ParameterDirection.Output)))
			{
				parameters[param.ParameterName] = param.HasValue
					? param.Value
					: param.IsNullable
						? null
						: param.ClrType.GetDefaultValue();
			}
		}
	}
}