using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource;

namespace Develop.API.Configuration;

public sealed class DevelopDataContextProvider : IPgSqlDataContextProvider
{
	private const string _defaultDataContextName = "default";

	private readonly ILogger<IDataContextProvider> _logger;

	private OptionsListener<SqlDbSettings>? _listener;
	private SqlDbSettings? _settings;
	private IPgSqlDataContext? _dataContext;
	private object? _syncRoot;

	private object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;
	public IEnumerable<DataContextInfo> Infos
	{
		get
		{
			yield return new DataContextInfo(_defaultDataContextName, () => PgSqlDataLayer.Default);
		}
	}


	/* 	public PgSqlDataContextProvider(IOptions<SqlDbSettings> options, ILogger<IDataContextProvider> logger)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			_settings = options.Value;
			_logger = logger;
		} */

	public DevelopDataContextProvider(OptionsListener<SqlDbSettings> optionsListener, ILogger<IDataContextProvider> logger)
	{
		if (optionsListener == null) throw new ArgumentNullException(nameof(optionsListener));

		_listener = optionsListener;
		_logger = logger;
	}

	public IDataContext GetDataContext(string dataContextName = _defaultDataContextName)
	{
		if (_listener == null && _settings == null) throw new ObjectDisposedException(nameof(DevelopDataContextProvider));
		if (dataContextName != _defaultDataContextName) throw new InvalidOperationException($"Unknown data context '{dataContextName}'.");

		if (_dataContext == null || _settings == null || (_listener != null && _settings != _listener.Value1))
		{
			IPgSqlDataContext? oldDataContext = null;
			bool isChanged = false;

			lock (SyncRoot)
			{
				if (_dataContext == null || _settings == null || (_listener != null && _settings != _listener.Value1))
				{
					var settings = _listener?.Value1 ?? _settings;

					var dataSourceBuilder = new NpgsqlDataSourceBuilder(_settings!.ConnectionString());
					var dataSource = dataSourceBuilder.Build();
					var dataContext = new PgSqlDataContext(dataSource, _defaultDataContextName);

					oldDataContext = _dataContext;
					isChanged = true;

					_dataContext = dataContext;
					_settings = settings;
				}
			}

			if (oldDataContext != null)
			{
				_logger.LogInformation($"PostgreSQL data context '{dataContextName}' has been changed to {_settings!.ToString()}");

				if (oldDataContext.Context is IAsyncDisposable disposable)
				{
					// Dispose oldDataContext after 2 minutes
					Task.Delay(120000).ContinueWith(async _ => await disposable.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
				}
			}
			else if (isChanged)
			{
				_logger.LogInformation($"PostgreSQL data context '{dataContextName}' has been set to {_settings!.ToString()}");
			}
		}

		return _dataContext;
	}

	public async ValueTask DisposeAsync()
	{
		if (_dataContext != null)
		{
			var context = _dataContext?.Context as IAsyncDisposable;

			_listener = null;
			_settings = null;
			_dataContext = null;

			if (context != null)
			{
				await context.DisposeAsync();
			}
		}
	}
}