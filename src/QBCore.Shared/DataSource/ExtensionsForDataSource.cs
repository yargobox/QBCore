using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class ExtensionsForDataSource
{
	public static Type GetDataSourceInterface(this Type dataSourceType)
		=> dataSourceType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new InvalidOperationException($"Invalid datasource type {dataSourceType.ToPretty()}.");
	public static Type GetDataSourceTKey(this Type dataSourceType)
		=> dataSourceType.GetDataSourceInterface().GetGenericArguments()[0];
	public static Type GetDataSourceTDoc(this Type dataSourceType)
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
	
	public static DSTypeInfo ToDSTypeInfo(this Type dataSourceType)
		=> new DSTypeInfo(dataSourceType);

	public static T AsInfo<T>(this IAppObjectInfo appObject)
	{
		if (appObject is not T tvalue)
		{
			throw new InvalidOperationException($"Property '{nameof(appObject)}' has type '{appObject.GetType().ToPretty()}' not requested '{typeof(T).ToPretty()}'.");
		}

		return tvalue;
	}

	public static IDSInfo AsDSInfo(this IAppObjectInfo appObject) => AsInfo<IDSInfo>(appObject);

	public static ICDSInfo AsCDSInfo(this IAppObjectInfo appObject) => AsInfo<ICDSInfo>(appObject);
}