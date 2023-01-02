using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource;

namespace Example1.DAL.Configuration;

public sealed class MongoDataContextProvider : IMongoDataContextProvider
{
	private const string _defaultDataContextName = "default";

	private readonly ILogger<IDataContextProvider> _logger;

	private OptionsListener<MongoDbSettings>? _listener;
	private MongoDbSettings? _settings;
	private IMongoDataContext? _dataContext;
	private object? _syncRoot;

	private object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;
	public IEnumerable<DataContextInfo> Infos
	{
		get
		{
			yield return new DataContextInfo(_defaultDataContextName, () => MongoDataLayer.Default);
		}
	}

/* 	public MongoDataContextProvider(IOptions<MongoDbSettings> options, ILogger<IDataContextProvider> logger)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));

		_settings = options.Value;
		_logger = logger;
	} */

	public MongoDataContextProvider(OptionsListener<MongoDbSettings> optionsListener, ILogger<IDataContextProvider> logger)
	{
		if (optionsListener == null) throw new ArgumentNullException(nameof(optionsListener));

		_listener = optionsListener;
		_logger = logger;
	}

	public IDataContext GetDataContext(string dataContextName = "default")
	{
		if (_listener == null && _settings == null) throw new ObjectDisposedException(nameof(MongoDataContextProvider));
		if (dataContextName != _defaultDataContextName) throw new InvalidOperationException($"Unknown data context '{dataContextName}'.");
	
		if (_dataContext == null || _settings == null || (_listener != null && _settings != _listener.Value1))
		{
			IMongoDataContext? oldDataContext = null;
			bool isChanged = false;

			lock (SyncRoot)
			{
				if (_dataContext == null || _settings == null || (_listener != null && _settings != _listener.Value1))
				{
					var settings = _listener?.Value1 ?? _settings;
					var client = new MongoClient(settings!.ConnectionString);
					var db = client.GetDatabase(settings.Catalog);
					var dataContext = new MongoDataContext(db, dataContextName);

					oldDataContext = _dataContext;
					isChanged = true;

					_dataContext = dataContext;
					_settings = settings;
				}
			}

			if (oldDataContext != null)
			{
				_logger.LogInformation($"Mongo data context '{dataContextName}' has changed to {_settings.ToString()}");

				if (oldDataContext.Context is IDisposable disposable)
				{
					// Dispose oldDataContext after 2 minutes
					Task.Delay(120000).ContinueWith(_ => disposable.Dispose()).ConfigureAwait(false);
				}
			}
			else if (isChanged)
			{
				_logger.LogInformation($"Mongo data context '{dataContextName}' has been set to {_settings.ToString()}");
			}
		}

		return _dataContext;
	}

	public void Dispose()
	{
		var context = _dataContext?.Context as IDisposable;

		_listener = null;
		_settings = null;
		_dataContext = null;

		context?.Dispose();
	}
}