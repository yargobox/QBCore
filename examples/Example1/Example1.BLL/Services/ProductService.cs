using Example1.DAL.Entities.Products;
using QBCore.DataSource;
using QBCore.Configuration;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("product")]
public sealed class ProductService : DataSource<int?, Product, ProductCreateDto, ProductSelectDto, ProductUpdateDto, EmptyDto, OrderService>
{
	public ProductService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }
}