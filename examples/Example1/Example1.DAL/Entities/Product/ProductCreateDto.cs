using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Products;

public class ProductCreateDto
{
	public string Name { get; set; } = null!;
	public int ProductId { get; set; }

	private static void InsertBuilder(IQBInsertBuilder<Product, ProductCreateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.InsertToTable("products");
	}
}