using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Orders;

public class OrderCreateDto
{
	public string? Name { get; set; }

	public List<OrderPosition>? OrderPositions { get; set; }

	private static void Builder(IQBInsertBuilder<Order, OrderCreateDto> builder)
	{
		//builder.InsertToTable("orders");
	}
}