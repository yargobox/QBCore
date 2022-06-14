using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Brands;

public class BrandUpdateDto
{
	public string? Name { get; set; }

	[EagerLoading]
	static BrandUpdateDto()
	{
		QueryBuilders.RegisterUpdate<Brand, BrandUpdateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.UpdateTable("brands");
		});
	}
}