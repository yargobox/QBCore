using Example1.DAL.Entities.Products;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPositionSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? ProductId { get; set; }
	public virtual ProductSelectDto? Product { get; set; }

	public decimal? Price { get; set; }
	public decimal? Quantity { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<OrderPosition, OrderPositionSelectDto> builder)
	{
		builder.SelectFromTable("order_positions");
	}
}