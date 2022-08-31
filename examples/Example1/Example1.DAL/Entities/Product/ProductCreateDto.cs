using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Products;

public class ProductCreateDto
{
	public string? Name { get; set; }
	public int? ProductId { get; set; }

	static void Builder(IQBMongoInsertBuilder<Product, ProductCreateDto> builder)
	{
		builder.Insert("products");
	}
}