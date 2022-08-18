namespace QBCore.DataSource;

public static class ExtensionsForDataSource
{
	public static Type GetDataSourceTKey(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[0];
	}
	public static Type GetDataSourceTDocument(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[1];
	}
	public static Type GetDataSourceTCreate(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[2];
	}
	public static Type GetDataSourceTSelect(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[3];
	}
	public static Type GetDataSourceTUpdate(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[4];
	}
	public static Type GetDataSourceTDelete(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[5];
	}
	public static Type GetDataSourceTRestore(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return genericArgs[6];
	}
	public static (Type TKey, Type TDocument, Type TCreate, Type TSelect, Type TUpdate, Type TDelete, Type TRestore) GetDataSourceTypes(this Type dataSourceType)
	{
		var dataSourceInterfaceType = dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
		var genericArgs = dataSourceInterfaceType.GetGenericArguments();
		return
		(
			TKey: genericArgs[0],
			TDocument: genericArgs[1],
			TCreate: genericArgs[2],
			TSelect: genericArgs[3],
			TUpdate: genericArgs[4],
			TDelete: genericArgs[5],
			TRestore: genericArgs[6]
		);
	}
}