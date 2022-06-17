namespace QBCore.DataSource;

public static class ExtensionsForDataSource
{
	public static Type GetDataSourceTKey(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[0];
	}
	public static Type GetDataSourceTDocument(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[1];
	}
	public static Type GetDataSourceTCreate(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[2];
	}
	public static Type GetDataSourceTSelect(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[3];
	}
	public static Type GetDataSourceTUpdate(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[4];
	}
	public static Type GetDataSourceTDelete(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[5];
	}
}