using Example1.DAL.Entities.Brands;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Products;

public class ProductSelectDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public int BrandId { get; set; }
	public Brand? Brand { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	private static void SelectBuilder(IQBSelectBuilder<Product, ProductSelectDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.SelectFromTable("products");
	}
}