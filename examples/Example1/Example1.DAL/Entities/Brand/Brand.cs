using Example1.DAL.Entities.Products;

namespace Example1.DAL.Entities.Brands;

public class Brand
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}