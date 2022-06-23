using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class DataSources
{
	public static void Register<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.Register(StaticFactory.DataSources, typeof(TDataSource));

	public static void Register(Type concreteType)
		=> DataSources.Register(StaticFactory.DataSources, concreteType);

	public static IFactoryObjectDictionary<Type, IDSDefinition> Register<TDataSource>(this IFactoryObjectDictionary<Type, IDSDefinition> @this)
		where TDataSource : IDataSource
		=> DataSources.Register(@this, typeof(TDataSource));
	
	public static IFactoryObjectDictionary<Type, IDSDefinition> Register(this IFactoryObjectDictionary<Type, IDSDefinition> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDSDefinition>)@this;
		registry.RegisterObject(concreteType, new DSDefinition(concreteType));
		return @this;
	}

	public static bool TryRegister<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.TryRegister(StaticFactory.DataSources, typeof(TDataSource));

	public static bool TryRegister(Type concreteType)
		=> DataSources.TryRegister(StaticFactory.DataSources, concreteType);

	public static bool TryRegister<TDataSource>(this IFactoryObjectDictionary<Type, IDSDefinition> @this)
		where TDataSource : IDataSource
		=> DataSources.TryRegister(@this, typeof(TDataSource));

	public static bool TryRegister(this IFactoryObjectDictionary<Type, IDSDefinition> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDSDefinition>)@this;
		return registry.TryRegisterObject(concreteType, new DSDefinition(concreteType));
	}
}