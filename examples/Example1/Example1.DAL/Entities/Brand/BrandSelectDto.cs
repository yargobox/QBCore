using Example1.DAL.Entities.Products;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Brands;

public class BrandSelectDto
{
	[DeId] public int? Id { get; set; }
	[DeViewName] public string? Name { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();

	static void Builder(IQBMongoSelectBuilder<Brand, BrandSelectDto> builder)
	{
		builder.Select("brands");
	}
}