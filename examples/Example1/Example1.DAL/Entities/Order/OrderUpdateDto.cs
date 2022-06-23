using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Orders;

public class OrderUpdateDto
{
	public string Name { get; set; } = null!;
	public decimal? Total { get; set; }

	private static void UpdateBuilder(IQBUpdateBuilder<Order, OrderUpdateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.UpdateTable("orders");
	}
}