using Example1.DAL.Entities.Products;
using QBCore.DataSource;

namespace Example1.DAL.Entities.OrderPositions;

public class OrderPosition
{
	[DeId] public int? Id { get; set; }
	[DeName] public string? Name { get; set; }

	[DeForeignId] public int? ProductId { get; set; }
	public virtual Product? Product { get; set; }

	public decimal? Price { get; set; }
	public decimal? Quantity { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }
}