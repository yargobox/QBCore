using Example1.DAL.Entities.Brands;
using QBCore.DataSource;
using QBCore.Configuration;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("brand", Listener = typeof(BrandServiceListener))]
public sealed class BrandService : DataSource<int?, Brand, BrandCreateDto, BrandSelectDto, BrandUpdateDto, EmptyDto, BrandService>
{
	public BrandService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }
}