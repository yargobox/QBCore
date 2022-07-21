using Example1.DAL.Entities.OrderPositions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QBCore.DataSource;

namespace Example1.DAL.Entities.Orders;

public class Order
{
	[DsId] public int? Id { get; set; }
	public string? Name { get; set; }

	[DsRef] public int? StoreId { get; set; }

	public List<OrderPosition>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	[DsCreated] public DateTimeOffset? Created { get; set; }
	[DsUpdated] public DateTimeOffset? Updated { get; set; }
	[DsDeleted] public DateTimeOffset? Deleted { get; set; }

	[BsonExtraElements]
	public BsonDocument? ExtraElements { get; set; }
}