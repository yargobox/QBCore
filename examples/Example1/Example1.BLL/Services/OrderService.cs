using Example1.DAL.Entities;
using Example1.DAL.Entities.Orders;
using QBCore.Configuration;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("order", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
public sealed class OrderService : DataSource<int?, Order, OrderCreateDto, OrderSelectDto, OrderUpdateDto, SoftDelDto, SoftDelDto, OrderService>
{
	public OrderService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

	static void DefinitionBuilder(IDSBuilder builder)
	{
		//builder.Name = "[DS]";
		//builder.Options |= DataSourceOptions.SoftDelete | DataSourceOptions.CanInsert | DataSourceOptions.CanSelect;
		//builder.DataContextName = "default";
		//builder.DataLayer = typeof(MongoDataLayer);
		//builder.IsAutoController = true;
		//builder.ControllerName = "[DS:guessPlural]";
		//builder.IsServiceSingleton = false;
		//builder.Listener = typeof(BrandServiceListener);
		//builder.ServiceInterface = null;
		//builder.InsertBuilder = BrandCreateDto.InsertBuilder;
		//builder.SelectBuilder = BrandSelectDto.SelectBuilder;
		//builder.UpdateBuilder = BrandUpdateDto.UpdateBuilder;
		//builder.SoftDelBuilder = SoftDelBuilder;
		//builder.RestoreBuilder = RestoreBuilder;
	}

	static void SoftDelBuilder(IQBSoftDelBuilder<Order, SoftDelDto> qb)
	{
		//qb.UpdateTable("orders");
	}
	static void RestoreBuilder(IQBRestoreBuilder<Order, SoftDelDto> qb)
	{
		//qb.UpdateTable("orders");
	}
}