using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class DataContextProvider : IDataContextProvider, IDisposable
{
	private const string _defaultContextName = "default";

	private ILogger<IDataContextProvider> _logger;
	private OptionsMonitor<MongoDbSettings> _mongoDbSettingsMonitor;
	private IDataContext? _mongoDataContext;
	public bool IsDisposed { get; private set; }

	public DataContextProvider(OptionsMonitor<MongoDbSettings> optionsMonitor, ILogger<IDataContextProvider> logger)
	{
		_logger = logger;
		_mongoDbSettingsMonitor = optionsMonitor;
	}

	public IDataContext GetDataContext(Type databaseContextType, string dataContextName = "default")
	{
		if (IsDisposed) throw new ObjectDisposedException(nameof(DataContextProvider));
		if (dataContextName != _defaultContextName) throw new InvalidOperationException($"Unknown data context '{dataContextName}'.");
	
		if (databaseContextType == typeof(IMongoDbContext))
		{
			if (_mongoDataContext == null)
			{
				var client = new MongoClient(_mongoDbSettingsMonitor.CurrentValue.ToString());
				var db = client.GetDatabase(_mongoDbSettingsMonitor.CurrentValue.Catalog);
				var mongoDbContext = new MongoDbContext(db);
				_mongoDataContext = new DataContext(mongoDbContext, dataContextName, null);
			}

			return _mongoDataContext;
		}

		throw new InvalidOperationException($"Unknown database context type '{databaseContextType.ToPretty()}'.");
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;

			var temp = _mongoDataContext;

			_mongoDbSettingsMonitor = null!;
			_mongoDataContext = null;

			(temp?.Context as IDisposable)?.Dispose();
		}
	}
}