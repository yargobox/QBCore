using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Example1.DAL.Entities.Orders;

public class Order
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? StoreId { get; set; }

	public List<OrderPosition>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	[BsonExtraElements]
	public BsonDocument? ExtraElements { get; set; }
}