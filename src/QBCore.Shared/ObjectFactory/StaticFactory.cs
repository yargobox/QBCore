using QBCore.DataSource;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IFactoryObjectDictionary<Type, Lazy<DSDocumentInfo>> Documents { get; } = new ConcurrentFactoryObjectRegistry<Type, Lazy<DSDocumentInfo>>();
	public static IFactoryObjectDictionary<Type, IDSInfo> DataSources => _dataSources;
	public static IFactoryObjectDictionary<Type, ICDSInfo> ComplexDataSources => _complexDataSources;
	public static IFactoryObjectDictionary<string, IAppObjectInfo> AppObjects => _appObjects;
	public static IFactoryObjectDictionary<string, IAppObjectInfo> AppObjectByControllerNames => _appObjectByControllerNames;
	public static IFactoryObjectDictionary<string, BusinessObject> BusinessObjects => _businessObjects;
	public static object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;

	private static FactoryObjectRegistry<Type, IDSInfo> _dataSources = new FactoryObjectRegistry<Type, IDSInfo>();
	private static FactoryObjectRegistry<Type, ICDSInfo> _complexDataSources = new FactoryObjectRegistry<Type, ICDSInfo>();
	private static FactoryObjectRegistry<string, IAppObjectInfo> _appObjects = new FactoryObjectRegistry<string, IAppObjectInfo>();
	private static FactoryObjectRegistry<string, IAppObjectInfo> _appObjectByControllerNames = new FactoryObjectRegistry<string, IAppObjectInfo>(StringComparer.OrdinalIgnoreCase);
	private static FactoryObjectRegistry<string, BusinessObject> _businessObjects = new FactoryObjectRegistry<string, BusinessObject>();
	private static object? _syncRoot;

	public static List<IDSInfo> RegisterRange(Func<Type, IDSInfo> factoryMethod, IEnumerable<Type> dataSourceTypes)
	{
		if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
		if (dataSourceTypes == null) throw new ArgumentNullException(nameof(dataSourceTypes));

		lock (SyncRoot)
		{
			var typesToRegister = dataSourceTypes.Where(x => !DataSources.ContainsKey(x)).ToArray();
			if (typesToRegister.Length == 0) Enumerable.Empty<Type>();

			var registered = new List<IDSInfo>(typesToRegister.Length);
			var newDataSources = new FactoryObjectRegistry<Type, IDSInfo>(DataSources, null);
			var newAppObjects = new FactoryObjectRegistry<string, IAppObjectInfo>(AppObjects, null);
			var newAppObjectByControllerNames = new FactoryObjectRegistry<string, IAppObjectInfo>(AppObjectByControllerNames, null);
			var newBusinessObjects = new FactoryObjectRegistry<string, BusinessObject>(BusinessObjects, null);

			IDSInfo pDSInfo;
			BusinessObject bo;
			foreach (var type in typesToRegister)
			{
				pDSInfo = factoryMethod(type);

				registered.Add(pDSInfo);

				newDataSources.RegisterObject(type, pDSInfo);
				
				newAppObjects.RegisterObject(pDSInfo.Name, pDSInfo);

				if (pDSInfo.ControllerName != null)
				{
					newAppObjectByControllerNames.RegisterObject(pDSInfo.ControllerName, pDSInfo);
				}

				bo = new BusinessObject("DS", string.Intern(pDSInfo.Name), pDSInfo.ConcreteType);
				newBusinessObjects.RegisterObject(bo.Key, bo);
			}

			_dataSources = newDataSources;
			_appObjects = newAppObjects;
			_appObjectByControllerNames = newAppObjectByControllerNames;
			_businessObjects = newBusinessObjects;

			return registered;
		}
	}

	public static IList<ICDSInfo> RegisterRange(Func<Type, ICDSInfo> factoryMethod, IEnumerable<Type> complexDataSourceTypes)
	{
		if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
		if (complexDataSourceTypes == null) throw new ArgumentNullException(nameof(complexDataSourceTypes));

		lock (SyncRoot)
		{
			var typesToRegister = complexDataSourceTypes.Where(x => !ComplexDataSources.ContainsKey(x)).ToArray();
			if (typesToRegister.Length == 0) new List<ICDSInfo>(0);

			var registered = new List<ICDSInfo>(typesToRegister.Length);
			var newComplexDataSources = new FactoryObjectRegistry<Type, ICDSInfo>(ComplexDataSources, null);
			var newAppObjects = new FactoryObjectRegistry<string, IAppObjectInfo>(AppObjects, null);
			var newAppObjectByControllerNames = new FactoryObjectRegistry<string, IAppObjectInfo>(AppObjectByControllerNames, null);
			var newBusinessObjects = new FactoryObjectRegistry<string, BusinessObject>(BusinessObjects, null);

			ICDSInfo pCDSInfo;
			BusinessObject bo;
			foreach (var type in typesToRegister)
			{
				pCDSInfo = factoryMethod(type);

				registered.Add(pCDSInfo);

				newComplexDataSources.RegisterObject(type, pCDSInfo);
				
				newAppObjects.RegisterObject(pCDSInfo.Name, pCDSInfo);

				if (pCDSInfo.ControllerName != null)
				{
					newAppObjectByControllerNames.RegisterObject(pCDSInfo.ControllerName, pCDSInfo);
				}

				bo = new BusinessObject("CDS", string.Intern(pCDSInfo.Name), pCDSInfo.ConcreteType);
				newBusinessObjects.RegisterObject(bo.Key, bo);
			}

			_complexDataSources = newComplexDataSources;
			_appObjects = newAppObjects;
			_appObjectByControllerNames = newAppObjectByControllerNames;
			_businessObjects = newBusinessObjects;

			return registered;
		}
	}
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