using Example1.DAL.Entities.Stores;
using QBCore.DataSource;
using QBCore.Configuration;
using Example1.DAL.Entities;
using QBCore.DataSource.QueryBuilder.Mongo;
using QBCore.DataSource.QueryBuilder;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("store", typeof(MongoQBFactory), DataSourceOptions.SoftDelete)]
public sealed class StoreService : DataSource<int?, Store, StoreCreateDto, StoreSelectDto, StoreUpdateDto, SoftDelDto, SoftDelDto, StoreService>
{
	public StoreService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

	static void SoftDelBuilder(IQBSoftDelBuilder<Store, SoftDelDto> qb)
	{
		qb.UpdateTable("stores");
	}
	static void RestoreBuilder(IQBRestoreBuilder<Store, SoftDelDto> qb)
	{
		qb.UpdateTable("stores");
	}
}