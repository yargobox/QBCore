using QBCore.DataSource;

namespace QBCore.ObjectFactory
{
	public static class ExtensionsForDSAndCDS
	{
		public static void RegisterObject(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType)
		{
			if (@this != StaticFactory.DataSources) throw new InvalidOperationException();

			var pDSInfo = new DSInfo(concreteType);

			var registry = (IFactoryObjectRegistry<Type, IDSInfo>)StaticFactory.DataSources;
			registry.RegisterObject(concreteType, pDSInfo);
		}

		public static IDSInfo GetOrRegisterObject(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType)
		{
			if (@this != StaticFactory.DataSources) throw new InvalidOperationException();

			var registry = (IFactoryObjectRegistry<Type, IDSInfo>)StaticFactory.DataSources;
			return registry.GetOrRegisterObject(concreteType, type => new DSInfo(type));
		}

		public static IDSInfo CreateDSInfo(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType)
		{
			return new DSInfo(concreteType);
		}

		public static void RegisterObject(this IFactoryObjectDictionary<Type, ICDSInfo> @this, Type concreteType)
		{
			if (@this != StaticFactory.ComplexDataSources) throw new InvalidOperationException();

			var pCDSInfo = new CDSInfo(concreteType);

			var registry = (IFactoryObjectRegistry<Type, ICDSInfo>)StaticFactory.ComplexDataSources;
			registry.RegisterObject(concreteType, pCDSInfo);
		}

		public static ICDSInfo GetOrRegisterObject(this IFactoryObjectDictionary<Type, ICDSInfo> @this, Type concreteType)
		{
			if (@this != StaticFactory.ComplexDataSources) throw new InvalidOperationException();

			var registry = (IFactoryObjectRegistry<Type, ICDSInfo>)StaticFactory.ComplexDataSources;
			return registry.GetOrRegisterObject(concreteType, (type) => new CDSInfo(type));
		}
	}
}

namespace QBCore.ObjectFactory.Internals
{
	public static class ExtensionsForDSAndCDS
	{
		public static IDSInfo DSInfoFactoryMethod(this IFactoryObjectDictionary<Type, IDSInfo> @this, Type concreteType) => new DSInfo(concreteType);
		public static ICDSInfo CDSInfoFactoryMethod(this IFactoryObjectDictionary<Type, ICDSInfo> @this, Type concreteType) => new CDSInfo(concreteType);
	}
}