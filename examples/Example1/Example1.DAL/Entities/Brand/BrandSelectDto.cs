using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Brands;

public class BrandSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	[EagerLoading]
	static BrandSelectDto()
	{
		QueryBuilders.RegisterSelect<Brand, BrandSelectDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.SelectFromTable("brands");
		});
	}
}