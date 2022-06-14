using Microsoft.Extensions.Options;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class SqlDbContext : ISqlDbContext, IDisposable
{
	object _DB;
	public object DB => _DB;
	public SqlDbSettings SqlDbSettings { get; }

	public SqlDbContext(SqlDbSettings settings)
	{
		SqlDbSettings = settings;

		_DB = null!;
	}

	public SqlDbContext(IOptions<SqlDbSettings> settings)
	{
		SqlDbSettings = settings.Value;

		_DB = null!;
	}

	static SqlDbContext()
	{
	}

	public void Dispose()
	{
		if (_DB is IDisposable dispose)
		{
			dispose.Dispose();
			_DB = null!;
		}
	}
}