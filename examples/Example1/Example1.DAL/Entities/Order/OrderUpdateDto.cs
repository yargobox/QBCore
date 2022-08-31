using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Orders;

public class OrderUpdateDto
{
	public string? Name { get; set; }
	public List<OrderPositionSelectDto>? OrderPositions { get; set; }
	public decimal? Total { get; set; }

	static void Builder(IQBMongoUpdateBuilder<Order, OrderUpdateDto> builder)
	{
		builder.Update("orders");
	}
}