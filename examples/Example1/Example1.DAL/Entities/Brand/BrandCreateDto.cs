using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Brands;

public class BrandCreateDto
{
	[MaxLength(60)]
	public string? Name { get; set; }

	public static void InsertBuilder(IQBInsertBuilder<Brand, BrandCreateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.InsertToTable("brands");
	}
}