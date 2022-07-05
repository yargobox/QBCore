using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Stores;

public class StoreUpdateDto
{
	public string? Name { get; set; }

	static void Builder(IQBUpdateBuilder<Store, StoreUpdateDto> builder)
	{
		//builder.UpdateTable("stores");
	}
}