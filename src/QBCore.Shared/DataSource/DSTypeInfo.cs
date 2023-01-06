namespace QBCore.DataSource;

public sealed class DSTypeInfo
{
	/// <summary>
    /// Concrete implementation class derived from DataSource<>
    /// </summary>
	public readonly Type Concrete;
	
	/// <summary>
    /// <see cref="IDataSource{}"/> interface that includes a generic <typeparamref name="TDoc"/> argument
    /// </summary>
	public readonly Type Interface;

	/// <summary>
    /// So-called "public" version of the IDataSource<> interface (without a generic <typeparamref name="TDoc"/> argument)
    /// </summary>
	public readonly Type Public;

	/// <summary>
    /// Key type
    /// </summary>
	public readonly Type TKey;

	/// <summary>
    /// Document type
    /// </summary>
	public readonly Type TDoc;

	/// <summary>
    /// Create DTO type or the NotSupported type
    /// </summary>
	public readonly Type TCreate;

	/// <summary>
    /// Select DTO type or the NotSupported type
    /// </summary>
	public readonly Type TSelect;

	/// <summary>
    /// Update DTO type or the NotSupported type
    /// </summary>
	public readonly Type TUpdate;

	/// <summary>
    /// Delete DTO type or the NotSupported type
    /// </summary>
	public readonly Type TDelete;

	/// <summary>
    /// Restore DTO type or the NotSupported type
    /// </summary>
	public readonly Type TRestore;

	public DSTypeInfo(Type dataSourceConcreteType)
	{
		if (dataSourceConcreteType == null) throw new ArgumentNullException(nameof(dataSourceConcreteType));

		Concrete = dataSourceConcreteType;
		Interface = dataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) ??
			throw new ArgumentException($"Invalid datasource type {dataSourceConcreteType.ToPretty()}.", nameof(dataSourceConcreteType));
		Public = dataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new ArgumentException($"Invalid datasource type {dataSourceConcreteType.ToPretty()}.", nameof(dataSourceConcreteType));

		var genericArgs = Interface.GetGenericArguments();
		TKey = genericArgs[0];
		TDoc = genericArgs[1];
		TCreate = genericArgs[2];
		TSelect = genericArgs[3];
		TUpdate = genericArgs[4];
		TDelete = genericArgs[5];
		TRestore = genericArgs[6];
	}

	public DSTypeInfo(Type Concrete, Type Interface, Type Public, Type TKey, Type TDoc, Type TCreate, Type TSelect, Type TUpdate, Type TDelete, Type TRestore)
	{
		this.Concrete = Concrete;
		this.Interface = Interface;
		this.Public = Public;
		this.TKey = TKey;
		this.TDoc = TDoc;
		this.TCreate = TCreate;
		this.TSelect = TSelect;
		this.TUpdate = TUpdate;
		this.TDelete = TDelete;
		this.TRestore = TRestore;
	}
}