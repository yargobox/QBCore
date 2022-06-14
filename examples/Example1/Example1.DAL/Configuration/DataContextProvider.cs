using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class DataContextProvider : IDataContextProvider, IDisposable
{
	ILogger<IDataContextProvider> _logger;
	private MongoDbContext? _mongoDbContext;
	private SqlDbContext? _sqlDbContext;
	private IDisposable? _mongoChangeHandle;
	private IDisposable? _sqlChangeHandle;

	public DataContextProvider(IOptionsMonitor<MongoDbSettings> mongo, IOptionsMonitor<SqlDbSettings> postgresql, ILogger<IDataContextProvider> logger)
	{
		_logger = logger;
		
		_mongoChangeHandle = mongo.OnChange(OnDatabaseSettingsChanged);
		_sqlChangeHandle = postgresql.OnChange(OnDatabaseSettingsChanged);

		OnDatabaseSettingsChanged(mongo.CurrentValue);
		OnDatabaseSettingsChanged(postgresql.CurrentValue);
	}

	private void OnDatabaseSettingsChanged(MongoDbSettings settings)
	{
		if (settings == _mongoDbContext?.MongoDbSettings) return;

		MongoDbContext newContext;
		try
		{
			newContext = new MongoDbContext(settings);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, $"Failed to set MongoDB context to {settings.User}:?@{settings.Host}:{settings.Port}/{settings.Catalog}");
			return;
		}

		MongoDbContext? oldContext;
		do
		{
			oldContext = _mongoDbContext;
		}
		while (Interlocked.CompareExchange(ref _mongoDbContext, newContext, oldContext) != oldContext);

		_logger.LogInformation($"Set MongoDB context to {settings.User}:?@{settings.Host}:{settings.Port}/{settings.Catalog}");

		if (oldContext is IDisposable)
		{	// Kill after 30 sec and break all running requests older than 30 sec
			Task.Delay(30 * 1000)
				.ContinueWith(x => { try { oldContext.Dispose(); } catch { } })
				.ConfigureAwait(false);
		}
	}
	private void OnDatabaseSettingsChanged(SqlDbSettings settings)
	{
		if (settings == _sqlDbContext?.SqlDbSettings) return;

		SqlDbContext newContext;
		try
		{
			newContext = new SqlDbContext(settings);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, $"Failed to set SQL context to {settings.User}:?@{settings.Host}:{settings.Port}/{settings.Catalog}");
			return;
		}

		SqlDbContext? oldContext;
		do
		{
			oldContext = _sqlDbContext;
		}
		while (Interlocked.CompareExchange(ref _sqlDbContext, newContext, oldContext) != oldContext);

		_logger.LogInformation($"Set SQL context to {settings.User}:?@{settings.Host}:{settings.Port}/{settings.Catalog}");

		if (oldContext is IDisposable)
		{	// Kill after 30 sec and break all running requests older than 30 sec
			Task.Delay(30 * 1000)
				.ContinueWith(x => { try { oldContext.Dispose(); } catch { } })
				.ConfigureAwait(false);
		}
	}

	public IDataContext GetContext<TDatabaseContext>(string dataContextName = "default")
		=> GetContext(typeof(TDatabaseContext), dataContextName);
	public IDataContext GetContext(Type databaseContextType, string dataContextName = "default")
	{
		if (dataContextName != "default")
		{
			throw new InvalidOperationException($"Unknown data context '{dataContextName}'.");
		}

		if (databaseContextType == typeof(IMongoDbContext))
		{
			if (_mongoDbContext != null)
			{
				return new DataContext(_mongoDbContext, null);
			}
			throw new InvalidProgramException("MongoDB context not set.");
		}
		else if (databaseContextType == typeof(ISqlDbContext))
		{
			if (_sqlDbContext != null)
			{
				return new DataContext(_sqlDbContext, null);
			}
			throw new InvalidProgramException("SQL context not set.");
		}

		throw new InvalidOperationException($"Unknown database context type '{databaseContextType.ToPretty()}'.");
	}

	public void Dispose()
	{
		Dispose(ref _mongoChangeHandle);
		Dispose(ref _sqlChangeHandle);
		Dispose(ref _mongoDbContext);
		Dispose(ref _sqlDbContext);
	}
	static void Dispose<T>(ref T? obj)
	{
		(obj as IDisposable)?.Dispose();
		obj = default(T?);
	}
}