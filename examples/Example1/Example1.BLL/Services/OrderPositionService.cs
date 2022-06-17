using Example1.DAL.Entities.OrderPositions;
using QBCore.DataSource;
using QBCore.Configuration;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("position")]
public sealed class OrderPositionService : DataSource<int?, OrderPosition, OrderPositionCreateDto, OrderPositionSelectDto, OrderPositionUpdateDto, EmptyDto, OrderPositionService>
{
	public OrderPositionService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }
}