using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class MongoDbContext : IMongoDbContext, IDisposable
{
	IMongoDatabase _DB;
	public IMongoDatabase DB => _DB;
	public MongoDbSettings MongoDbSettings { get; }

	public MongoDbContext(MongoDbSettings settings)
	{
		MongoDbSettings = settings;
		
		var client = new MongoClient(MongoDbSettings.ToString());
		_DB = client.GetDatabase(MongoDbSettings.Catalog);
	}

	public MongoDbContext(IOptions<MongoDbSettings> settings)
	{
		MongoDbSettings = settings.Value;

		var client = new MongoClient(MongoDbSettings.ToString());
		_DB = client.GetDatabase(MongoDbSettings.Catalog);
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