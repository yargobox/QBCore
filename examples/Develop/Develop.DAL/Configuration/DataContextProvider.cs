using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QBCore.Configuration;
using QBCore.Extensions.Reflection;

namespace Develop.DAL.Configuration;

public sealed class DataContextProvider : IDataContextProvider, IDesignTimeDbContextFactory<DbDevelopContext>, IDisposable
{
	private const string _defaultContextName = "default";
	private const string _appSettingsRelFilePath = "appsettings.json";

	private OptionsMonitor<SqlDbSettings>? _sqlDbSettingsMonitor;
	private SqlDbSettings? _sqlDbSettings;
	private IDataContext? _dataContext;
	public bool IsDisposed { get; private set; }

	public DataContextProvider()
	{
	}

/* 	public DataContextProvider(IOptions<SqlDbSettings> options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));

		_sqlDbSettings = options.Value;
	} */

	public DataContextProvider(OptionsMonitor<SqlDbSettings> optionsMonitor)
	{
		if (optionsMonitor == null) throw new ArgumentNullException(nameof(optionsMonitor));

		_sqlDbSettingsMonitor = optionsMonitor;
	}

	public IDataContext GetDataContext(Type databaseContextType, string dataContextName = _defaultContextName)
	{
		if (IsDisposed) throw new ObjectDisposedException(nameof(DataContextProvider));
		if (dataContextName != _defaultContextName) throw new InvalidOperationException($"Unknown data context '{dataContextName}'.");

		if (databaseContextType == typeof(IEntityFrameworkDbContext))
		{
			if (_dataContext == null)
			{
				if (_sqlDbSettingsMonitor != null)
				{
					var setup = new DbContextOptionsBuilder<DbDevelopContext>();
					setup.UseNpgsql(_sqlDbSettingsMonitor.Value.ToString());
					var dbDevelopContext = new DbDevelopContext(setup.Options);
					var sqlDbContext = new SqlDbContext(dbDevelopContext);
					_dataContext = new DataContext(sqlDbContext, dataContextName, null);
				}
				else
				{
					var dbDevelopContext = CreateDbContext(null!);
					var sqlDbContext = new SqlDbContext(dbDevelopContext);
					_dataContext = new DataContext(sqlDbContext, dataContextName, null);
				}
			}

			return _dataContext;
		}

		throw new InvalidOperationException($"Unknown database context type '{databaseContextType.ToPretty()}'.");
	}

	public DbDevelopContext CreateDbContext(string[] args)
	{
		if (IsDisposed) throw new ObjectDisposedException(nameof(DataContextProvider));

		if (_sqlDbSettingsMonitor == null && _sqlDbSettings == null)
		{
			var filePath = Path.Combine(Environment.CurrentDirectory, _appSettingsRelFilePath);
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile(filePath);
			var configurationRoot = configurationBuilder.Build();
			_sqlDbSettings = configurationRoot.GetRequiredSection(nameof(SqlDbSettings)).Get<SqlDbSettings>()
				?? throw new InvalidOperationException($"Database context settings '{nameof(SqlDbSettings)}' is not set in the file '{filePath}'.");
		}

		var setup = new DbContextOptionsBuilder<DbDevelopContext>();
		setup.UseNpgsql((_sqlDbSettingsMonitor?.Value ?? _sqlDbSettings!).ToString());
		return new DbDevelopContext(setup.Options);
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;

			var temp = _dataContext;

			_sqlDbSettingsMonitor = null;
			_sqlDbSettings = null;
			_dataContext = null;

			(temp?.Context as IDisposable)?.Dispose();
		}
	}
}