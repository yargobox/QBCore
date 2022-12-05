using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace QBCore.Controllers.Tests;

public class GlobalTestFixture : IDisposable
{
	public IServiceProvider ServiceProvider { get; set; }

	public GlobalTestFixture()
	{
		BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
		BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
		BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.String));
		ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);

		var scopeFactory = new DefaultServiceProviderFactory(new ServiceProviderOptions { ValidateScopes = true });
		var services = scopeFactory.CreateBuilder(new ServiceCollection())
			.AddQBCore(null, typeof(GlobalTestFixture).Assembly);

		ServiceProvider = scopeFactory.CreateServiceProvider(services);
	}

	public void Dispose()
	{
		var temp = ServiceProvider as IDisposable;
		ServiceProvider = null!;
		temp?.Dispose();
	}
}

[CollectionDefinition(nameof(GlobalTestFixture))]
public class GlobalTestCollectionFixture : ICollectionFixture<GlobalTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}