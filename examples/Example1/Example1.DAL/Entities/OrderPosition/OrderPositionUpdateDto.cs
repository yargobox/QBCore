using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionUpdateDto
{
	public string Name { get; set; } = null!;
	public int ProductId { get; set; }
	public decimal Quantity { get; set; }

	[EagerLoading]
	static OrderPositionUpdateDto()
	{
		QueryBuilders.RegisterUpdate<OrderPosition, OrderPositionUpdateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.UpdateTable("orderpositions");
		});
	}
}