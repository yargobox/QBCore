namespace QBCore.DataSource;

public static class ExtensionsForDataSource
{
	public static Type GetDataSourceInterface(this Type dataSourceType)
		=> dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
	public static Type GetDataSourceTKey(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[0];
	public static Type GetDataSourceTDocument(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[1];
	public static Type GetDataSourceTCreate(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[2];
	public static Type GetDataSourceTSelect(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[3];
	public static Type GetDataSourceTUpdate(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[4];
	public static Type GetDataSourceTDelete(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[5];
	public static Type GetDataSourceTRestore(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[6];
}