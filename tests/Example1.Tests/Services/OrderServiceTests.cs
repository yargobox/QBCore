using Example1.BLL.Services;
using Example1.DAL.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
public class OrderService_Tests
{
	[Fact]
	public async Task UnitOfWork_StateUnderTest_ExpectedBehavior()
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

		var scopeFactory = new DefaultServiceProviderFactory(new ServiceProviderOptions { ValidateScopes = true });
		var services =
			scopeFactory.CreateBuilder(new ServiceCollection())
			.AddQBCore()
			.AddTransient<ITransient<IMongoDataContextProvider>>(sp => mongoDataContextProvider.Object)
			.AddSingleton<IMongoDataContextProvider>(sp => mongoDataContextProvider.Object)
			.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDataContextProvider>().GetDataContext().AsMongoDatabase());

		var serviceProvider = scopeFactory.CreateServiceProvider(services);
		using var serviceProviderDisposable = serviceProvider as IDisposable;

		using var ds = serviceProvider.GetRequiredService<OrderService>();

		await Task.CompletedTask;
	}
}