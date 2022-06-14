using Example1.DAL.Entities.Brands;

namespace Example1.DAL.Entities.Products;

public class Product
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public int BrandId { get; set; }
	public Brand? Brand { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}