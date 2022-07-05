using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Brands;

public class BrandCreateDto
{
	public string? Name { get; set; }

	public static void Builder(IQBInsertBuilder<Brand, BrandCreateDto> builder)
	{
		//builder.InsertToTable("brands");
	}
}