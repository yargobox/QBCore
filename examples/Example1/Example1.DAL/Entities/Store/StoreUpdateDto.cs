using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Stores;

public class StoreUpdateDto
{
	public string Name { get; set; } = null!;

	[EagerLoading]
	static StoreUpdateDto()
	{
		QueryBuilders.RegisterUpdate<Store, StoreUpdateDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.UpdateTable("stores");
		});
	}
}