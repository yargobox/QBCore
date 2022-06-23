using Example1.DAL.Entities.Products;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionSelectDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public int ProductId { get; set; }
	public Product? Product { get; set; }

	public decimal? Price { get; set; }
	public decimal Quantity { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	private static void SelectBuilder(IQBSelectBuilder<OrderPosition, OrderPositionSelectDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.SelectFromTable("order_positions");
	}
}