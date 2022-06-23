using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	public string Name { get; set; } = null!;

	private static void InsertBuilder(IQBInsertBuilder<Store, StoreCreateDto> builder)
	{
		builder.Map(c =>
		{
			c.AutoMap();
		});

		builder.InsertToTable("stores");
	}
}