using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Orders;

public class OrderSelectDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public List<OrderPosition> OrderPositions { get; set; } = null!;

	public decimal? Total { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	private static void SelectBuilder(IQBSelectBuilder<Order, OrderSelectDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.SelectFromTable("orders");
	}
}