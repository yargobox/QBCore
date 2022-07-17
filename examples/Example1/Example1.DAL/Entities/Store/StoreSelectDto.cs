using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Stores;

public class StoreSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<Store, StoreSelectDto> builder)
	{
		builder.SelectFrom("stores");
	}
}