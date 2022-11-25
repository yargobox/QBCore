using Example1.DAL.Entities.Products;
using QBCore.DataSource;
using QBCore.Configuration;
using Example1.DAL.Entities;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("product", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
public sealed class ProductService : DataSource<int?, Product, ProductCreateDto, ProductSelectDto, ProductUpdateDto, SoftDelDto, SoftDelDto, OrderService>
{
	public ProductService(IServiceProvider serviceProvider) : base(serviceProvider) { }

	static void SoftDelBuilder(IQBMongoSoftDelBuilder<Product, SoftDelDto> qb)
	{
		qb.Update("products");
	}
	static void RestoreBuilder(IQBMongoRestoreBuilder<Product, SoftDelDto> qb)
	{
		qb.Update("products");
	}
}