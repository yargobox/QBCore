using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Brands;

public class BrandCreateDto
{
	public string? Name { get; set; }

	public static void Builder(IQBMongoInsertBuilder<Brand, BrandCreateDto> builder)
	{
		builder
			.InsertTo("brands")
			.IdGenerator = () => new PesemisticSequentialIdGenerator<Brand>(1, 1, 8);
	}
}