using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Orders;

public class OrderUpdateDto
{
	public string? Name { get; set; }
	public List<OrderPositionSelectDto>? OrderPositions { get; set; }
	public decimal? Total { get; set; }

	static void Builder(IQBUpdateBuilder<Order, OrderUpdateDto> builder)
	{
		//builder.UpdateTable("orders");
	}
}