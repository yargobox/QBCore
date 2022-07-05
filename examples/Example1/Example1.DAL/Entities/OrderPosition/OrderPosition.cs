using Example1.DAL.Entities.Products;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPosition
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? ProductId { get; set; }
	public virtual Product? Product { get; set; }

	public decimal? Price { get; set; }
	public decimal? Quantity { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}