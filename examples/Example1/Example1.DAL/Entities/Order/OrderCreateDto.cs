using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Orders;

public class OrderCreateDto
{
	public string Name { get; set; } = null!;

	private static void InsertBuilder(IQBInsertBuilder<Order, OrderCreateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.InsertToTable("orders");
	}
}