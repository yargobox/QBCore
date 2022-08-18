using Example1.DAL.Entities;
using Example1.DAL.Entities.Brands;
using QBCore.Configuration;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.BLL.Services;

[DsApiController]
[DataSource("brand", typeof(MongoDataLayer), DataSourceOptions.SoftDelete, Listener = typeof(BrandServiceListener))]
public sealed class BrandService : DataSource<int?, Brand, BrandCreateDto, BrandSelectDto, BrandUpdateDto, SoftDelDto, SoftDelDto, BrandService>
{
	public BrandService(IServiceProvider serviceProvider, IDataContextProvider dataContextProvider) : base(serviceProvider, dataContextProvider) { }

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
	static void SoftDelBuilder(IQBMongoSoftDelBuilder<Brand, SoftDelDto> qb)
	{
		//qb.UpdateTable("brands");
	}
	static void RestoreBuilder(IQBMongoRestoreBuilder<Brand, SoftDelDto> qb)
	{
		//qb.UpdateTable("brands");
	}
}