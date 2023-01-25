using Example1.DAL.Entities.Products;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionSelectDto
{
	[DeId] public int? Id { get; set; }
	[DeName] public string? Name { get; set; }

	[DeForeignId] public int? ProductId { get; set; }
	public virtual ProductSelectDto? Product { get; set; }

	public decimal? Price { get; set; }
	public decimal? Quantity { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	static void Builder(IMongoSelectQBBuilder<OrderPosition, OrderPositionSelectDto> builder)
	{
		builder.Select("order_positions");
	}
}