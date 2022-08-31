using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	public string? Name { get; set; }

	static void Builder(IQBMongoInsertBuilder<Store, StoreCreateDto> builder)
	{
		builder.Insert("stores");
	}
}