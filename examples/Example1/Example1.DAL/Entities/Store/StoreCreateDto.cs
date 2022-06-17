using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Stores;

public class StoreCreateDto
{
	public string Name { get; set; } = null!;

	[EagerLoading]
	static StoreCreateDto()
	{
		QueryBuilders.RegisterInsert<Store, StoreCreateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.InsertToTable("stores");
		});
	}
}