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
	public StoreService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

	static void SoftDelBuilder(IQBMongoSoftDelBuilder<Store, SoftDelDto> qb)
	{
		//qb.UpdateTable("stores");
	}
	static void RestoreBuilder(IQBMongoRestoreBuilder<Store, SoftDelDto> qb)
	{
		//qb.UpdateTable("stores");
	}
}