using Example1.DAL.Entities.OrderPositions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QBCore.DataSource;

namespace Example1.DAL.Entities.Orders;

public class Order
{
	[DeId] public int? Id { get; set; }
	public string? Name { get; set; }

	[DeForeignId] public int? StoreId { get; set; }

	public List<OrderPosition>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	[BsonExtraElements]
	public BsonDocument? ExtraElements { get; set; }
}