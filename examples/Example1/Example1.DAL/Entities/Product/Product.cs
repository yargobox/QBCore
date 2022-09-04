using Example1.DAL.Entities.Brands;
using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource;

namespace Example1.DAL.Entities.Products;

public class Product
{
	[DeId] public int? Id { get; set; }
	public string? Name { get; set; }

	public int? BrandId { get; set; }
	public Brand? Brand { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	public virtual IEnumerable<OrderPosition> OrderPositions { get; set; } = new List<OrderPosition>();
}