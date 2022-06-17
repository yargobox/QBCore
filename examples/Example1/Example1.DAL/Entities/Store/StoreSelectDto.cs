using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Runtime;

namespace Example1.DAL.Entities.Stores;

public class StoreSelectDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	[EagerLoading]
	static StoreSelectDto()
	{
		QueryBuilders.RegisterSelect<Store, StoreSelectDto>(qb =>
		{
			qb.Map(c =>
			{
				c.AutoMap();
			});

			qb.SelectFromTable("stores");
		});
	}
}