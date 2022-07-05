using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionUpdateDto
{
	public string? Name { get; set; } = null!;
	public int? ProductId { get; set; }
	public decimal? Quantity { get; set; }

	private static void Builder(IQBUpdateBuilder<OrderPosition, OrderPositionUpdateDto> builder)
	{
		//builder.UpdateTable("order_positions");
	}
}