using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Stores;

public class StoreSelectDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	private static void SelectBuilder(IQBSelectBuilder<Store, StoreSelectDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.SelectFromTable("stores");
	}
}