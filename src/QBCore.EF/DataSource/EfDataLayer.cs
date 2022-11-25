using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.EntityFramework;

namespace QBCore.DataSource;

public sealed class EfDataLayer : IDataLayerInfo
{
	public static IDataLayerInfo Default => SingletonInstance.Instance;

	public string Name => "EntityFramework";
	public Type DataContextInterfaceType => typeof(IEfDataContext);
	public Type DataContextProviderInterfaceType => typeof(IEfDataContextProvider);
	
	public Func<Type, bool> IsDocumentType
	{
		get => _isDocumentType;
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Interlocked.Exchange(ref _isDocumentType, value);
		}
	}
	private Func<Type, bool> _isDocumentType;

	public Func<Type, string> GetDefaultDBSideContainerName
	{
		get => _getDefaultDBSideContainerName;
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Interlocked.Exchange(ref _getDefaultDBSideContainerName, value);
		}
	}
	private Func<Type, string> _getDefaultDBSideContainerName;

	private EfDataLayer()
	{
		_isDocumentType = IsDocumentTypeImplementation;
		_getDefaultDBSideContainerName = type => throw new NotSupportedException(nameof(GetDefaultDBSideContainerName) + " is not supported by EF data layer.");
	}

	public DSDocumentInfo CreateDocumentInfo(Type documentType)
	{
		return new EfDocumentInfo(documentType);
	}

	public IQueryBuilderFactory CreateQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		return new EfQBFactory(dsTypeInfo, options, insertBuilderMethod, selectBuilderMethod, updateBuilderMethod, deleteBuilderMethod, softDelBuilderMethod, restoreBuilderMethod, lazyInitialization);
	}

	private bool IsDocumentTypeImplementation(Type type)
	{
		if (type.IsEnum || type.IsGenericTypeDefinition) return false;

		if (type.IsValueType)
		{
			if (_standardValueTypes.Contains(type)) return false;
		}
		else if (type.IsClass)
		{
			if (_standardRefTypes.Contains(type)) return false;
		}

		if (DataSourceDocuments.DocumentExclusionSelector(type))
		{
			return false;
		}

		if (type.GetInterfaceOf(typeof(IEnumerable)) != null || type.GetInterfaceOf(typeof(IEnumerable<>)) != null)
		{
			return false;
		}

		if (type.IsDefined(typeof(DsNotDocumentAttribute), true))
		{
			return false;
		}

		return true;
	}

	static class SingletonInstance
	{
		public static readonly EfDataLayer Instance = new EfDataLayer();

		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static SingletonInstance() { }
	}

	private static readonly HashSet<Type> _standardValueTypes = new HashSet<Type>()
	{
		typeof(bool),
		typeof(byte),
		typeof(sbyte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(Guid),
		typeof(DateOnly),
		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(TimeSpan),
		typeof(IntPtr),
		typeof(UIntPtr)
	};

	private static readonly HashSet<Type> _standardRefTypes = new HashSet<Type>()
	{
		typeof(NotSupported),
		typeof(string),
		typeof(object),
		typeof(bool?),
		typeof(byte?),
		typeof(sbyte?),
		typeof(byte[]),
		typeof(short?),
		typeof(ushort?),
		typeof(int?),
		typeof(uint?),
		typeof(long?),
		typeof(ulong?),
		typeof(float?),
		typeof(Single?),
		typeof(double?),
		typeof(decimal?),
		typeof(Guid?),
		typeof(DateOnly?),
		typeof(DateTime?),
		typeof(DateTimeOffset?),
		typeof(TimeSpan?),
		typeof(IntPtr?),
		typeof(UIntPtr?),
		typeof(Regex)
	};
}