using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Brands;

public class BrandUpdateDto
{
	public string? Name { get; set; }

	public static void Builder(IQBMongoUpdateBuilder<Brand, BrandUpdateDto> builder)
	{
		//builder.UpdateTable("brands");
	}
}