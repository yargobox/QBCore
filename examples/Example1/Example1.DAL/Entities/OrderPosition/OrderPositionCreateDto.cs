using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionCreateDto
{
	public string Name { get; set; } = null!;

	public int ProductId { get; set; }
	public decimal Quantity { get; set; }

	[EagerLoading]
	static OrderPositionCreateDto()
	{
		QueryBuilders.RegisterInsert<OrderPosition, OrderPositionCreateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.InsertToTable("order_positions");
		});
	}
}