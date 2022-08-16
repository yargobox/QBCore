using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class DataSources
{
	public static IFactoryObjectDictionary<Type, IDSInfo> Collection => StaticFactory.DataSources;

	public static void Register<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.Register(StaticFactory.DataSources, typeof(TDataSource));

	public static void Register(Type concreteType)
		=> DataSources.Register(StaticFactory.DataSources, concreteType);

	public static IFactoryObjectDictionary<Type, IDSInfo> Register<TDataSource>(this IFactoryObjectDictionary<Type, IDSInfo> @this)
		where TDataSource : IDataSource
		=> DataSources.Register(@this, typeof(TDataSource));
	
	public static IFactoryObjectDictionary<Type, IDSInfo> Register(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDSInfo>)@this;
		registry.RegisterObject(concreteType, new DSInfo(concreteType));
		return @this;
	}

	public static bool TryRegister<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.TryRegister(StaticFactory.DataSources, typeof(TDataSource));

	public static bool TryRegister(Type concreteType)
		=> DataSources.TryRegister(StaticFactory.DataSources, concreteType);

	public static bool TryRegister<TDataSource>(this IFactoryObjectDictionary<Type, IDSInfo> @this)
		where TDataSource : IDataSource
		=> DataSources.TryRegister(@this, typeof(TDataSource));

	public static bool TryRegister(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDSInfo>)@this;
		return registry.TryRegisterObject(concreteType, new DSInfo(concreteType));
	}
}