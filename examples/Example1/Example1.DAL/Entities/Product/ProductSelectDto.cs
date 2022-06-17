using Example1.DAL.Entities.Brands;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

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

	[EagerLoading]
	static ProductSelectDto()
	{
		QueryBuilders.RegisterSelect<Product, ProductSelectDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.SelectFromTable("products");
		});
	}
}