using Example1.DAL.Entities.Brands;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Products;

public class ProductSelectDto
{
	[DeId] public int? Id { get; set; }
	[DeViewName] public string? Name { get; set; }

	[DeForeignId] public int? BrandId { get; set; }
	public BrandSelectDto? Brand { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<Product, ProductSelectDto> builder)
	{
		builder.Select("products");
	}
}