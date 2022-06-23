using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Products;

public class ProductUpdateDto
{
	public string Name { get; set; } = null!;
	public int BrandId { get; set; }

	private static void UpdateBuilder(IQBUpdateBuilder<Product, ProductUpdateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.UpdateTable("products");
	}
}