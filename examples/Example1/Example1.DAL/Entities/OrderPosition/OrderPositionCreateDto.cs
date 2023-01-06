using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionCreateDto
{
	[DeViewName] public string? Name { get; set; }

	[DeForeignId] public int? ProductId { get; set; }
	public decimal? Quantity { get; set; }

	static void Builder(IMongoInsertQBBuilder<OrderPosition, OrderPositionCreateDto> builder)
	{
		builder.Insert("order_positions");
	}
}