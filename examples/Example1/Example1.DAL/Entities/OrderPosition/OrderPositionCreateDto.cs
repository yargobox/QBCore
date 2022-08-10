using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionCreateDto
{
	public string? Name { get; set; }

	public int? ProductId { get; set; }
	public decimal? Quantity { get; set; }

	static void Builder(IQBMongoInsertBuilder<OrderPosition, OrderPositionCreateDto> builder)
	{
		//builder.InsertToTable("order_positions");
	}
}