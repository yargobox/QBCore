using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Products;

public class ProductUpdateDto
{
	[DeViewName] public string? Name { get; set; }
	[DeForeignId] public int? BrandId { get; set; }

	static void Builder(IMongoUpdateQBBuilder<Product, ProductUpdateDto> builder)
	{
		builder.Update("products");
	}
}