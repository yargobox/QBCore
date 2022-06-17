using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Orders;

public class OrderUpdateDto
{
	public string Name { get; set; } = null!;
	public decimal? Total { get; set; }

	[EagerLoading]
	static OrderUpdateDto()
	{
		QueryBuilders.RegisterUpdate<Order, OrderUpdateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.UpdateTable("orders");
		});
	}
}