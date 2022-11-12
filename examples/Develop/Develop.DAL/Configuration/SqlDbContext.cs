using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;

namespace Develop.DAL.Configuration;

public sealed class SqlDbContext : IEntityFrameworkDbContext, IDisposable
{
	public DbContext Context => _context ?? throw new ObjectDisposedException(nameof(SqlDbContext));
	
	private DbContext? _context;

	public SqlDbContext(DbContext context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		_context = context;
	}

	public void Dispose()
	{
		var temp = _context;
		_context = null;
		if (temp is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}