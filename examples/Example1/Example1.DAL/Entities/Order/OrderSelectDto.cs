using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

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

	[EagerLoading]
	static OrderSelectDto()
	{
		QueryBuilders.RegisterSelect<Order, OrderSelectDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.SelectFromTable("orders");
		});
	}
}