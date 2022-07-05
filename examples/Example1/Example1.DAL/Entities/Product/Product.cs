using Example1.DAL.Entities.Brands;
using Example1.DAL.Entities.OrderPositions;

namespace Example1.DAL.Entities.Products;

public class Product
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? BrandId { get; set; }
	public Brand? Brand { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	public virtual IEnumerable<OrderPosition> OrderPositions { get; set; } = new List<OrderPosition>();
}