using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Orders;

public class OrderCreateDto
{
	public string Name { get; set; } = null!;

	[EagerLoading]
	static OrderCreateDto()
	{
		QueryBuilders.RegisterInsert<Order, OrderCreateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.InsertToTable("orders");
		});
	}
}