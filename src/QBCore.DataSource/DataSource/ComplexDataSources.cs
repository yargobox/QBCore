using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class ComplexDataSources
{
	public static IFactoryObjectDictionary<Type, ICDSInfo> Collection => StaticFactory.ComplexDataSources;

	public static void Register<TComplexDataSource>()
		where TComplexDataSource : IComplexDataSource
		=> ComplexDataSources.Register(StaticFactory.ComplexDataSources, typeof(TComplexDataSource));

	public static void Register(Type concreteType)
		=> ComplexDataSources.Register(StaticFactory.ComplexDataSources, concreteType);

	public static IFactoryObjectDictionary<Type, ICDSInfo> Register<TComplexDataSource>(this IFactoryObjectDictionary<Type, ICDSInfo> @this)
		where TComplexDataSource : IComplexDataSource
		=> ComplexDataSources.Register(@this, typeof(TComplexDataSource));
	
	public static IFactoryObjectDictionary<Type, ICDSInfo> Register(this IFactoryObjectDictionary<Type, ICDSInfo> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, ICDSInfo>)@this;
		registry.RegisterObject(concreteType, new CDSInfo(concreteType));
		return @this;
	}

	public static bool TryRegister<TComplexDataSource>()
		where TComplexDataSource : IComplexDataSource
		=> ComplexDataSources.TryRegister(StaticFactory.ComplexDataSources, typeof(TComplexDataSource));

	public static bool TryRegister(Type concreteType)
		=> ComplexDataSources.TryRegister(StaticFactory.ComplexDataSources, concreteType);

	public static bool TryRegister<TComplexDataSource>(this IFactoryObjectDictionary<Type, ICDSInfo> @this)
		where TComplexDataSource : IComplexDataSource
		=> ComplexDataSources.TryRegister(@this, typeof(TComplexDataSource));

	public static bool TryRegister(this IFactoryObjectDictionary<Type, ICDSInfo> @this, Type concreteType)
	{
		var registry = (IFactoryObjectRegistry<Type, ICDSInfo>)@this;
		return registry.TryRegisterObject(concreteType, new CDSInfo(concreteType));
	}
}