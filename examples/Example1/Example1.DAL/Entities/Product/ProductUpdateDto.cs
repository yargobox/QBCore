using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Products;

public class ProductUpdateDto
{
	public string Name { get; set; } = null!;
	public int BrandId { get; set; }

	[EagerLoading]
	static ProductUpdateDto()
	{
		QueryBuilders.RegisterUpdate<Product, ProductUpdateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.UpdateTable("products");
		});
	}
}