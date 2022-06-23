using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionCreateDto
{
	public string Name { get; set; } = null!;

	public int ProductId { get; set; }
	public decimal Quantity { get; set; }

	private static void InsertBuilder(IQBInsertBuilder<OrderPosition, OrderPositionCreateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.InsertToTable("order_positions");
	}
}