using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreUpdateDto
{
	[DeName] public string? Name { get; set; }

	static void Builder(IMongoUpdateQBBuilder<Store, StoreUpdateDto> builder)
	{
		builder.Update("stores");
	}
}