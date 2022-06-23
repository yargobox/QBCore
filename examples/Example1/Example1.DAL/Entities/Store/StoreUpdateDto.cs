using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Stores;

public class StoreUpdateDto
{
	public string Name { get; set; } = null!;

	private static void UpdateBuilder(IQBUpdateBuilder<Store, StoreUpdateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.UpdateTable("stores");
	}
}