using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource;
using QBCore.Configuration;
using Example1.DAL.Entities;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("position", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
public sealed class OrderPositionService : DataSource<int?, OrderPosition, OrderPositionCreateDto, OrderPositionSelectDto, OrderPositionUpdateDto, SoftDelDto, SoftDelDto, OrderPositionService>
{
	public OrderPositionService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

	static void SoftDelBuilder(IQBMongoSoftDelBuilder<OrderPosition, SoftDelDto> qb)
	{
		//qb.UpdateTable("order_positions");
	}
	static void RestoreBuilder(IQBMongoRestoreBuilder<OrderPosition, SoftDelDto> qb)
	{
		//qb.UpdateTable("order_positions");
	}
}