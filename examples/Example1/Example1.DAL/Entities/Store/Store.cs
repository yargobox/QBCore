using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Example1.DAL.Entities.Stores;

public class Store
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	[BsonExtraElements]
	public BsonDocument? ExtraElements { get; set; }
}