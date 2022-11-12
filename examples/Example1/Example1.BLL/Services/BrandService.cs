using Example1.DAL.Entities;
using Example1.DAL.Entities.Brands;
using QBCore.Configuration;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.BLL.Services;

[DsApiController]
[DsListeners(typeof(BrandServiceListener))]
[DataSource("brand", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
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
		//builder.Listeners.Add(typeof(BrandServiceListener));
		//builder.ServiceInterface = null;
		//builder.InsertBuilder = BrandCreateDto.InsertBuilder;
		//builder.SelectBuilder = BrandSelectDto.SelectBuilder;
		//builder.UpdateBuilder = BrandUpdateDto.UpdateBuilder;
		//builder.SoftDelBuilder = SoftDelBuilder;
		//builder.RestoreBuilder = RestoreBuilder;
	}
/* 	static void SoftDelBuilder(IQBMongoSoftDelBuilder<Brand, SoftDelDto> qb)
	{
		qb.Update("brands")
			.Condition(doc => doc.Id, FO.Equal, "id")
			.Condition(doc => doc.Deleted, null, FO.IsNull)
		;
	}
	static void RestoreBuilder(IQBMongoRestoreBuilder<Brand, SoftDelDto> qb)
	{
		qb.Update("brands")
			.Condition(doc => doc.Id, FO.Equal, "id")
			.Condition(doc => doc.Deleted, null, FO.IsNotNull)
		;
	}
	static void DeleteBuilder(IQBMongoDeleteBuilder<Brand, SoftDelDto> qb)
	{
		qb.Delete("brands")
			.Condition(doc => doc.Id, FO.Equal, "id");
	} */
}