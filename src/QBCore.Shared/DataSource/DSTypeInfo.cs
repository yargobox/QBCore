namespace QBCore.DataSource;

public sealed class DSTypeInfo
{
	public readonly Type Concrete;
	public readonly Type Interface;
	public readonly Type TKey;
	public readonly Type TDocument;
	public readonly Type TCreate;
	public readonly Type TSelect;
	public readonly Type TUpdate;
	public readonly Type TDelete;
	public readonly Type TRestore;

	public DSTypeInfo(Type dataSourceConcreteType)
	{
		if (dataSourceConcreteType == null)
		{
			throw new ArgumentNullException(nameof(dataSourceConcreteType));
		}

		Concrete = dataSourceConcreteType;
		Interface = dataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new ArgumentException($"Invalid datasource type {dataSourceConcreteType.ToPretty()}.", nameof(dataSourceConcreteType));

		var genericArgs = Interface.GetGenericArguments();
		TKey = genericArgs[0];
		TDocument = genericArgs[1];
		TCreate = genericArgs[2];
		TSelect = genericArgs[3];
		TUpdate = genericArgs[4];
		TDelete = genericArgs[5];
		TRestore = genericArgs[6];
	}
}