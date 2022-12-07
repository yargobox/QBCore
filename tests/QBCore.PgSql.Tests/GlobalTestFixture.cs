using Microsoft.Extensions.DependencyInjection;

namespace QBCore.Controllers.Tests;

public class GlobalTestFixture : IDisposable
{
	public IServiceProvider ServiceProvider { get; set; }

	public GlobalTestFixture()
	{
		var services = new ServiceCollection();
		services
			.AddQBCore(null, typeof(GlobalTestFixture).Assembly);

		ServiceProvider = services.BuildServiceProvider(true);
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