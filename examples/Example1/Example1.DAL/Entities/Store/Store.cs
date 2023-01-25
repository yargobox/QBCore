using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QBCore.DataSource;

namespace Example1.DAL.Entities.Stores;

public class Store
{
	[DeId] public int? Id { get; set; }
	[DeName] public string? Name { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	[BsonExtraElements]
	public BsonDocument? ExtraElements { get; set; }
}