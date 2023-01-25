using Example1.DAL.Entities.Products;
using QBCore.DataSource;

namespace Example1.DAL.Entities.Brands;

public class Brand
{
	[DeId] public int? Id { get; set; }
	[DeName] public string? Name { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();
}