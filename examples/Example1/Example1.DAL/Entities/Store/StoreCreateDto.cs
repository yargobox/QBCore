using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	[DeViewName] public string? Name { get; set; }

	static void Builder(IQBMongoInsertBuilder<Store, StoreCreateDto> builder)
	{
		builder.Insert("stores");
	}
}