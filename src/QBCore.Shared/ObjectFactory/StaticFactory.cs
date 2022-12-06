using System.Collections.Concurrent;
using System.Reflection;
using QBCore.Configuration;
using QBCore.DataSource;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IReadOnlyDictionary<Type, Lazy<DSDocumentInfo>> Documents => SingletonInstance._documents;
	public static IReadOnlyDictionary<Type, IDSInfo> DataSources => SingletonInstance._dataSources;
	public static IReadOnlyDictionary<Type, ICDSInfo> ComplexDataSources => SingletonInstance._complexDataSources;
	public static IReadOnlyDictionary<string, IAppObjectInfo> AppObjects => SingletonInstance._appObjects;
	public static IReadOnlyDictionary<string, IAppObjectInfo> AppObjectByControllerNames => SingletonInstance._appObjectByControllerNames;
	public static IReadOnlyDictionary<string, BusinessObject> BusinessObjects => SingletonInstance._businessObjects;


	private static class SingletonInstance
	{
		public static readonly Func<Assembly, bool> _defaultAssemblySelector = _ => true;
		public static readonly Func<Type, bool> _defaultDocumentExclusionSelector = _ => false;
		public static readonly Func<Type, bool> _defaultTypeSelector = _ => true;

		public static readonly ConcurrentDictionary<Type, Lazy<DSDocumentInfo>> _documents = new ConcurrentDictionary<Type, Lazy<DSDocumentInfo>>();
		public static IReadOnlyDictionary<Type, IDSInfo> _dataSources = new Dictionary<Type, IDSInfo>().AsReadOnly();
		public static IReadOnlyDictionary<Type, ICDSInfo> _complexDataSources = new Dictionary<Type, ICDSInfo>().AsReadOnly();
		public static IReadOnlyDictionary<string, IAppObjectInfo> _appObjects = new Dictionary<string, IAppObjectInfo>().AsReadOnly();
		public static IReadOnlyDictionary<string, IAppObjectInfo> _appObjectByControllerNames = new Dictionary<string, IAppObjectInfo>(StringComparer.OrdinalIgnoreCase).AsReadOnly();
		public static IReadOnlyDictionary<string, BusinessObject> _businessObjects = new Dictionary<string, BusinessObject>().AsReadOnly();
		public static object _syncRoot = new object();
		public static volatile Func<Type, bool> _documentExclusionSelector = _defaultDocumentExclusionSelector;
		public static volatile IReadOnlyList<Type> _dataContextProviders = new List<Type>(0).AsReadOnly();

		static SingletonInstance() { }
	}

	public static class Internals
	{
		public static Func<Assembly, bool> DefaultAssemblySelector => SingletonInstance._defaultAssemblySelector;
		public static Func<Type, bool> DefaultDocumentExclusionSelector => SingletonInstance._defaultDocumentExclusionSelector;
		public static Func<Type, bool> DefaultTypeSelector => SingletonInstance._defaultTypeSelector;

		public static IReadOnlyList<Type> DataContextProviders => SingletonInstance._dataContextProviders;
		public static Func<Type, bool> DocumentExclusionSelector => SingletonInstance._documentExclusionSelector;

		public static void AddDocumentExclusionSelector(Func<Type, bool> documentExclusionSelector)
		{
			if (documentExclusionSelector == null) throw new ArgumentNullException(nameof(documentExclusionSelector));

			if (documentExclusionSelector == DefaultDocumentExclusionSelector) return;

			lock (SingletonInstance._syncRoot)
			{
				var current = SingletonInstance._documentExclusionSelector;
				SingletonInstance._documentExclusionSelector = current != DefaultDocumentExclusionSelector
					? type => documentExclusionSelector(type) && current(type)
					: documentExclusionSelector;
			}
		}

		public static List<Type> RegisterDataContextProviders(IEnumerable<Type> dataContextProviders)
		{
			if (dataContextProviders == null) throw new ArgumentNullException(nameof(dataContextProviders));

			lock (SingletonInstance._syncRoot)
			{
				var typesToRegister = dataContextProviders.Except(SingletonInstance._dataContextProviders).ToList();

				if (typesToRegister.Count == 0) return typesToRegister;

				var newDataContextProviders = new List<Type>(SingletonInstance._dataContextProviders.Count + typesToRegister.Count);
				newDataContextProviders.AddRange(SingletonInstance._dataContextProviders);
				newDataContextProviders.AddRange(typesToRegister);

				SingletonInstance._dataContextProviders = typesToRegister.AsReadOnly();

				return typesToRegister;
			}
		}

		public static List<IDSInfo> RegisterDataSources(Func<Type, IDSInfo> factoryMethod, IEnumerable<Type> dataSourceTypes)
		{
			if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
			if (dataSourceTypes == null) throw new ArgumentNullException(nameof(dataSourceTypes));

			lock (SingletonInstance._syncRoot)
			{
				var typesToRegister = dataSourceTypes.Where(x => !SingletonInstance._dataSources.ContainsKey(x)).ToArray();
				var registered = new List<IDSInfo>(typesToRegister.Length);

				if (typesToRegister.Length == 0) return registered;

				var newDataSources = new Dictionary<Type, IDSInfo>(SingletonInstance._dataSources, null);
				newDataSources.EnsureCapacity(newDataSources.Count + typesToRegister.Length);

				var newAppObjects = new Dictionary<string, IAppObjectInfo>(SingletonInstance._appObjects, null);
				newAppObjects.EnsureCapacity(newAppObjects.Count + typesToRegister.Length);

				var newAppObjectByControllerNames = new Dictionary<string, IAppObjectInfo>(SingletonInstance._appObjectByControllerNames, StringComparer.OrdinalIgnoreCase);
				newAppObjectByControllerNames.EnsureCapacity(newAppObjectByControllerNames.Count + typesToRegister.Length);

				var newBusinessObjects = new Dictionary<string, BusinessObject>(SingletonInstance._businessObjects, null);
				newBusinessObjects.EnsureCapacity(newBusinessObjects.Count + typesToRegister.Length);

				IDSInfo pDSInfo;
				BusinessObject bo;
				foreach (var type in typesToRegister)
				{
					pDSInfo = factoryMethod(type);

					registered.Add(pDSInfo);
					newDataSources.Add(type, pDSInfo);
					newAppObjects.Add(pDSInfo.Name, pDSInfo);
					if (pDSInfo.ControllerName != null)
					{
						newAppObjectByControllerNames.Add(pDSInfo.ControllerName, pDSInfo);
					}

					bo = new BusinessObject("DS", string.Intern(pDSInfo.Name), pDSInfo.ConcreteType);
					newBusinessObjects.Add(bo.Key, bo);
				}

				SingletonInstance._dataSources = newDataSources.AsReadOnly();
				SingletonInstance._appObjects = newAppObjects.AsReadOnly();
				SingletonInstance._appObjectByControllerNames = newAppObjectByControllerNames.AsReadOnly();
				SingletonInstance._businessObjects = newBusinessObjects.AsReadOnly();

				return registered;
			}
		}

		public static IList<ICDSInfo> RegisterComplexDataSources(Func<Type, ICDSInfo> factoryMethod, IEnumerable<Type> complexDataSourceTypes)
		{
			if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
			if (complexDataSourceTypes == null) throw new ArgumentNullException(nameof(complexDataSourceTypes));

			lock (SingletonInstance._syncRoot)
			{
				var typesToRegister = complexDataSourceTypes.Where(x => !SingletonInstance._complexDataSources.ContainsKey(x)).ToArray();
				var registered = new List<ICDSInfo>(typesToRegister.Length);

				if (typesToRegister.Length == 0) return registered;

				var newComplexDataSources = new Dictionary<Type, ICDSInfo>(SingletonInstance._complexDataSources, null);
				newComplexDataSources.EnsureCapacity(newComplexDataSources.Count + typesToRegister.Length);

				var newAppObjects = new Dictionary<string, IAppObjectInfo>(SingletonInstance._appObjects, null);
				newAppObjects.EnsureCapacity(newAppObjects.Count + typesToRegister.Length);

				var newAppObjectByControllerNames = new Dictionary<string, IAppObjectInfo>(SingletonInstance._appObjectByControllerNames, StringComparer.OrdinalIgnoreCase);
				newAppObjectByControllerNames.EnsureCapacity(newAppObjectByControllerNames.Count + typesToRegister.Length);

				var newBusinessObjects = new Dictionary<string, BusinessObject>(SingletonInstance._businessObjects, null);
				newBusinessObjects.EnsureCapacity(newBusinessObjects.Count + typesToRegister.Length);

				ICDSInfo pCDSInfo;
				BusinessObject bo;
				foreach (var type in typesToRegister)
				{
					pCDSInfo = factoryMethod(type);

					registered.Add(pCDSInfo);
					newComplexDataSources.Add(type, pCDSInfo);
					newAppObjects.Add(pCDSInfo.Name, pCDSInfo);
					if (pCDSInfo.ControllerName != null)
					{
						newAppObjectByControllerNames.Add(pCDSInfo.ControllerName, pCDSInfo);
					}

					bo = new BusinessObject("CDS", string.Intern(pCDSInfo.Name), pCDSInfo.ConcreteType);
					newBusinessObjects.Add(bo.Key, bo);
				}

				SingletonInstance._complexDataSources = newComplexDataSources.AsReadOnly();
				SingletonInstance._appObjects = newAppObjects.AsReadOnly();
				SingletonInstance._appObjectByControllerNames = newAppObjectByControllerNames.AsReadOnly();
				SingletonInstance._businessObjects = newBusinessObjects.AsReadOnly();

				return registered;
			}
		}

		public static Lazy<DSDocumentInfo> GetOrRegisterDocument(Type documentType, IDataLayerInfo dataLayer)
		{
			var registry = (ConcurrentDictionary<Type, Lazy<DSDocumentInfo>>)StaticFactory.Documents;

			var doc = registry.GetValueOrDefault(documentType);
			if (doc == null)
			{
				foreach (var selectedType in GetDocumentReferencingTypes(documentType, dataLayer.IsDocumentType, true))
				{
					// a new var for each type to do not mess up with types in the lambda expression below
					var type = selectedType;
					if (doc == null)
						doc = registry.GetOrAdd(type, x => new Lazy<DSDocumentInfo>(() => dataLayer.CreateDocumentInfo(type), LazyThreadSafetyMode.ExecutionAndPublication));
					else
						registry.GetOrAdd(type, x => new Lazy<DSDocumentInfo>(() => dataLayer.CreateDocumentInfo(type), LazyThreadSafetyMode.ExecutionAndPublication));
				}

				if (doc == null)
				{
					throw new InvalidOperationException($"Could not register '{documentType.ToPretty()}' as a datasource document type.");
				}
			}
			return doc;
		}

		public static IEnumerable<Type> GetDocumentReferencingTypes(Type documentType, Func<Type, bool> documentTypesSelector, bool includeThisOne = false)
		{
			if (documentType == null)
			{
				throw new ArgumentNullException(nameof(documentType));
			}
			if (documentTypesSelector == null)
			{
				throw new ArgumentNullException(nameof(documentTypesSelector));
			}

			if (!documentTypesSelector(documentType))
			{
				return Enumerable.Empty<Type>();
			}

			var pool = new Dictionary<Type, bool>();
			if (includeThisOne)
			{
				pool.Add(documentType, true);
			}

			GetDocumentReferencingTypes(documentType, documentTypesSelector, pool);

			return pool.Where(x => x.Value).Select(x => x.Key);
		}
		private static void GetDocumentReferencingTypes(Type documentType, Func<Type, bool> documentTypesSelector, Dictionary<Type, bool> pool)
		{
			bool isDocumentType;
			Type type;

			foreach (var candidate in DSDocumentInfo.GetDataEntryCandidates(documentType))
			{
				if (candidate.memberInfo is PropertyInfo propertyInfo)
				{
					type = propertyInfo.PropertyType;
				}
				else if (candidate.memberInfo is FieldInfo fieldInfo)
				{
					type = fieldInfo.FieldType;
				}
				else
				{
					continue;
				}

				isDocumentType = documentTypesSelector(type);
				if (pool.TryAdd(type, isDocumentType))
				{
					if (isDocumentType)
					{
						GetDocumentReferencingTypes(type, documentTypesSelector, pool);
					}
					else
					{
						foreach (var genericEnumerable in type.GetInterfacesOf(typeof(IEnumerable<>)))
						{
							type = genericEnumerable.GetGenericArguments()[0];
							isDocumentType = documentTypesSelector(type);
							if (pool.TryAdd(type, isDocumentType) && isDocumentType)
							{
								GetDocumentReferencingTypes(type, documentTypesSelector, pool);
							}
						}
					}
				}
			}
		}
	}
}