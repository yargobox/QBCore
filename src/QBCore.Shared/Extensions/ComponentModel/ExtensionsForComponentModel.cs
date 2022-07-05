namespace QBCore.Extensions.ComponentModel;

public static class ExtensionsForComponentModel
{
	public static T? GetInstance<T>(this IServiceProvider provider)
		=> (T?)provider.GetService(typeof(T));

	public static T GetRequiredInstance<T>(this IServiceProvider provider)
		=> (T)provider.GetRequiredInstance(typeof(T));

	public static object GetRequiredInstance(this IServiceProvider provider, Type serviceType)
		=> provider.GetService(serviceType) ?? throw new InvalidOperationException($"Unable to resolve service for type '{serviceType.ToPretty()}'.");
}