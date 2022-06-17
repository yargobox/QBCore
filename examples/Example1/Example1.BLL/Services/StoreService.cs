using Example1.DAL.Entities.Stores;
using QBCore.DataSource;
using QBCore.Configuration;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("store")]
public sealed class StoreService : DataSource<int?, Store, StoreCreateDto, StoreSelectDto, StoreUpdateDto, EmptyDto, StoreService>
{
	public StoreService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }
}