using Example1.DAL.Entities.Stores;
using QBCore.DataSource;
using QBCore.Configuration;
using Example1.DAL.Entities;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("store", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
public sealed class StoreService : DataSource<int?, Store, StoreCreateDto, StoreSelectDto, StoreUpdateDto, SoftDelDto, SoftDelDto, StoreService>
{
	public StoreService(IServiceProvider serviceProvider) : base(serviceProvider) { }

	static void SoftDelBuilder(IMongoSoftDelQBBuilder<Store, SoftDelDto> qb)
	{
		qb.Update("stores");
	}
	static void RestoreBuilder(IMongoRestoreQBBuilder<Store, SoftDelDto> qb)
	{
		qb.Update("stores");
	}
}