using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Products;

public class ProductCreateDto
{
	[DeViewName] public string? Name { get; set; }
	[DeForeignId] public int? ProductId { get; set; }

	static void Builder(IQBMongoInsertBuilder<Product, ProductCreateDto> builder)
	{
		builder.Insert("products");
	}
}