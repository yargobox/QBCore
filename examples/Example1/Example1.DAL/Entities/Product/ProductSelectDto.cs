using Example1.DAL.Entities.Brands;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Products;

public class ProductSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? BrandId { get; set; }
	public BrandSelectDto? Brand { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<Product, ProductSelectDto> builder)
	{
		builder.Select("products");
	}
}