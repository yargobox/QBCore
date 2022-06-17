using Example1.DAL.Entities.Products;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

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

	[EagerLoading]
	static OrderPositionSelectDto()
	{
		QueryBuilders.RegisterSelect<OrderPosition, OrderPositionSelectDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.SelectFromTable("order_positions");
		});
	}
}