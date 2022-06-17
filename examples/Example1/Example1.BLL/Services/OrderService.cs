using Example1.DAL.Entities.Orders;
using QBCore.DataSource;
using QBCore.Configuration;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("order")]
public sealed class OrderService : DataSource<int?, Order, OrderCreateDto, OrderSelectDto, OrderUpdateDto, EmptyDto, OrderService>
{
	public OrderService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }
}