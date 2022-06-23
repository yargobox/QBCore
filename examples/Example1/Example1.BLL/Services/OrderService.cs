using Example1.DAL.Entities.Orders;
using QBCore.DataSource;
using QBCore.Configuration;
using Example1.DAL.Entities;
using QBCore.DataSource.QueryBuilder.Mongo;
using QBCore.DataSource.QueryBuilder;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("order", typeof(MongoQBFactory), DataSourceOptions.SoftDelete)]
public sealed class OrderService : DataSource<int?, Order, OrderCreateDto, OrderSelectDto, OrderUpdateDto, SoftDelDto, SoftDelDto, OrderService>
{
	public OrderService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

	static void SoftDelBuilder(IQBSoftDelBuilder<Order, SoftDelDto> qb)
	{
		qb.UpdateTable("orders");
	}
	static void RestoreBuilder(IQBRestoreBuilder<Order, SoftDelDto> qb)
	{
		qb.UpdateTable("orders");
	}
}