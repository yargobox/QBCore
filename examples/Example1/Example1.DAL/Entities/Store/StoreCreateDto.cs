using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	[DeName] public string? Name { get; set; }

	static void Builder(IMongoInsertQBBuilder<Store, StoreCreateDto> builder)
	{
		builder.Insert("stores");
	}
}