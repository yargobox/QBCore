using MongoDB.Driver;

namespace QBCore.Configuration;

public interface IMongoDbContext
{
	IMongoDatabase DB { get; }
}