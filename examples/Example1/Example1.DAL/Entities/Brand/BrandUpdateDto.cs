using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Brands;

public class BrandUpdateDto
{
	public string? Name { get; set; }

	public static void UpdateBuilder(IQBUpdateBuilder<Brand, BrandUpdateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.UpdateTable("brands");
	}
}