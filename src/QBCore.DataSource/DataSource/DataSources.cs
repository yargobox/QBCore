using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class DataSources
{
	public static void Register<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.Register(StaticFactory.DataSources, typeof(TDataSource));

	public static void Register(Type concreteType)
		=> DataSources.Register(StaticFactory.DataSources, concreteType);

	public static IFactoryObjectDictionary<Type, IDataSourceDesc> Register<TDataSource>(this IFactoryObjectDictionary<Type, IDataSourceDesc> @this)
		where TDataSource : IDataSource
		=> DataSources.Register(@this, typeof(TDataSource));
	
	public static IFactoryObjectDictionary<Type, IDataSourceDesc> Register(this IFactoryObjectDictionary<Type, IDataSourceDesc> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDataSourceDesc>)@this;
		registry.RegisterObject(concreteType, new DataSourceDesc(concreteType));
		return @this;
	}

	public static bool TryRegister<TDataSource>()
		where TDataSource : IDataSource
		=> DataSources.TryRegister(StaticFactory.DataSources, typeof(TDataSource));

	public static bool TryRegister(Type concreteType)
		=> DataSources.TryRegister(StaticFactory.DataSources, concreteType);

	public static bool TryRegister<TDataSource>(this IFactoryObjectDictionary<Type, IDataSourceDesc> @this)
		where TDataSource : IDataSource
		=> DataSources.TryRegister(@this, typeof(TDataSource));

	public static bool TryRegister(this IFactoryObjectDictionary<Type, IDataSourceDesc> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, IDataSourceDesc>)@this;
		return registry.TryRegisterObject(concreteType, new DataSourceDesc(concreteType));
	}
}