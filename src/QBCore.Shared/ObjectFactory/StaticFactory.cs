using QBCore.DataSource;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IFactoryObjectDictionary<Type, Lazy<DSDocumentInfo>> Documents { get; } = new ConcurrentFactoryObjectRegistry<Type, Lazy<DSDocumentInfo>>();
	public static IFactoryObjectDictionary<Type, IDSInfo> DataSources { get; } = new FactoryObjectRegistry<Type, IDSInfo>();
	public static IFactoryObjectDictionary<Type, ICDSInfo> ComplexDataSources { get; } = new FactoryObjectRegistry<Type, ICDSInfo>();
	public static IFactoryObjectDictionary<string, IAppObjectInfo> AppObjects { get; } = new FactoryObjectRegistry<string, IAppObjectInfo>();
	public static IFactoryObjectDictionary<string, IAppObjectInfo> AppObjectByControllerNames { get; } = new FactoryObjectRegistry<string, IAppObjectInfo>(StringComparer.OrdinalIgnoreCase);

	public static IFactoryObjectDictionary<string, BusinessObject> BusinessObjects { get; } = new FactoryObjectRegistry<string, BusinessObject>();
}

public static class ExtensionsForAppObject
{
	public static void RegisterObject(this IFactoryObjectDictionary<string, IAppObjectInfo> @this, IAppObjectInfo appObject)
	{
		if (!TryRegisterObject(@this, appObject))
		{
			throw new ArgumentException(nameof(appObject));
		}
	}

	public static bool TryRegisterObject(this IFactoryObjectDictionary<string, IAppObjectInfo> @this, IAppObjectInfo appObject)
	{
		if (appObject == null) throw new ArgumentNullException(nameof(appObject));
		if (@this != StaticFactory.AppObjects) throw new InvalidOperationException();

		var registry = (IFactoryObjectRegistry<string, IAppObjectInfo>)StaticFactory.AppObjects;
		var result = registry.TryRegisterObject(appObject.Name, appObject);

		if (result && appObject.ControllerName != null)
		{
			var registryByNames = (IFactoryObjectRegistry<string, IAppObjectInfo>)StaticFactory.AppObjectByControllerNames;
			registryByNames.RegisterObject(appObject.ControllerName, appObject);
		}

		return result;
	}
}