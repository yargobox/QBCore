using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionUpdateDto
{
	[DeName] public string? Name { get; set; } = null!;
	[DeForeignId] public int? ProductId { get; set; }
	public decimal? Quantity { get; set; }

	private static void Builder(IMongoUpdateQBBuilder<OrderPosition, OrderPositionUpdateDto> builder)
	{
		builder.Update("order_positions");
	}
}