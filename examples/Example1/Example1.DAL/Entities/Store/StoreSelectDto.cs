using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreSelectDto
{
	[DeId] public int? Id { get; set; }
	[DeViewName] public string? Name { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

	static void Builder(IMongoSelectQBBuilder<Store, StoreSelectDto> builder)
	{
		builder.Select("stores");
	}
}