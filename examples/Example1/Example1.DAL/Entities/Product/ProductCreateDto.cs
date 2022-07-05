using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Products;

public class ProductCreateDto
{
	public string? Name { get; set; }
	public int? ProductId { get; set; }

	static void Builder(IQBInsertBuilder<Product, ProductCreateDto> builder)
	{
		//builder.InsertToTable("products");
	}
}