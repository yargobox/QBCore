using MongoDB.Driver;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class MongoDbContext : IMongoDbContext, IDisposable
{
	public IMongoDatabase DB => _db ?? throw new ObjectDisposedException(nameof(MongoDbContext));
	
	private IMongoDatabase? _db;

	public MongoDbContext(IMongoDatabase db)
	{
		if (db == null) throw new ArgumentNullException(nameof(db));

		_db = db;
	}

	public void Dispose()
	{
		var temp = _db;
		_db = null;
		if (temp is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}