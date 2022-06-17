using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Products;

public class ProductCreateDto
{
	public string Name { get; set; } = null!;
	public int ProductId { get; set; }

	[EagerLoading]
	static ProductCreateDto()
	{
		QueryBuilders.RegisterInsert<Product, ProductCreateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.InsertToTable("products");
		});
	}
}