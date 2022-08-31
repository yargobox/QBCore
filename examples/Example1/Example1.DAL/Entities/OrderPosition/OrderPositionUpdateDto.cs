using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionUpdateDto
{
	public string? Name { get; set; } = null!;
	public int? ProductId { get; set; }
	public decimal? Quantity { get; set; }

	private static void Builder(IQBMongoUpdateBuilder<OrderPosition, OrderPositionUpdateDto> builder)
	{
		builder.Update("order_positions");
	}
}