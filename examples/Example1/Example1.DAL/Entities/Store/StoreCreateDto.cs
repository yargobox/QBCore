using QBCore.DataSource.QueryBuilder;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	public string? Name { get; set; }

	static void Builder(IQBInsertBuilder<Store, StoreCreateDto> builder)
	{
		//builder.InsertToTable("stores");
	}
}