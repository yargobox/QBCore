using System.ComponentModel.DataAnnotations;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Brands;

public class BrandCreateDto
{
	[MaxLength(60)]
	public string? Name { get; set; }

	[EagerLoading]
	static BrandCreateDto()
	{
		QueryBuilders.RegisterInsert<Brand, BrandCreateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.InsertToTable("brands");
		});
	}
}