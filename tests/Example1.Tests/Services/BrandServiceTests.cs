using Example1.BLL.Services;
using Example1.DAL.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.Controllers;
using QBCore.Controllers.Tests;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace Example1.BLL.Services.Tests;

[Collection(nameof(GlobalTestFixture))]
public class BrandServices_Tests
{
	[Fact]
	public async Task BrandService_StateUnderTest_ExpectedBehavior()
	{
		var mongoDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);
		var mongoDataContextProvider = new Mock<IMongoDataContextProvider>(MockBehavior.Strict);
		mongoDataContextProvider
			.Setup(dcp => dcp.Infos)
			.Returns(new DataContextInfo[]
				{
					new DataContextInfo("default", typeof(IMongoDatabase), () => MongoDataLayer.Default)
				});
		mongoDataContextProvider
			.Setup(dcp => dcp.GetDataContext(It.Is((string x) => x == "default")))
			.Returns(() => new MongoDataContext(mongoDatabase.Object));
		mongoDataContextProvider
			.Setup(dcp => dcp.Dispose());

		var services = new ServiceCollection();
		services
			.AddQBCore(null, typeof(Example1.DAL.Entities.Brands.Brand).Assembly, typeof(Example1.BLL.Services.BrandService).Assembly)
			.AddDataSourcesAsServices(StaticFactory.DataSources.Values)
			.AddTransient<ITransient<IMongoDataContextProvider>>(sp => mongoDataContextProvider.Object)
			.AddSingleton<IMongoDataContextProvider>(sp => mongoDataContextProvider.Object)
			.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDataContextProvider>().GetDataContext().AsMongoDatabase())
			.AddAutoMapper(config =>
			{
				config.AddProfile(new DataSourceMappings((source, dest) => true));
			});
		
		using var rootProvider = services.BuildServiceProvider(true);
		using var scope = rootProvider.CreateScope();
		var serviceProvider = scope.ServiceProvider;

		var brands = serviceProvider.GetRequiredService<BrandService>();
		using var transBrands = (BrandService) serviceProvider.GetRequiredService<ITransient<BrandService>>();

		await Task.CompletedTask;
	}
}