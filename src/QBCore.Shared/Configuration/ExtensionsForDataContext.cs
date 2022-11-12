namespace QBCore.Configuration;

public static class ExtensionsForDataContext
{
	public static T AsContext<T>(this IDataContext dataContext) where T : class
	{
		return (dataContext?.Context as T)
			?? throw new InvalidOperationException($"Couldn't get data context of type '{typeof(T).ToPretty()}' from '{dataContext?.Context?.GetType()?.ToPretty() ?? "null"}'.");
	}

	public static IDataContext GetDataContext<T>(this IDataContextProvider provider, string dataContextName = "default")
	{
		return provider.GetDataContext(typeof(T), dataContextName);
	}
}