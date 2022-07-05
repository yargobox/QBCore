using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Products;

public class ProductUpdateDto
{
	public string? Name { get; set; }
	public int? BrandId { get; set; }

	static void Builder(IQBUpdateBuilder<Product, ProductUpdateDto> builder)
	{
		//builder.UpdateTable("products");
	}
}