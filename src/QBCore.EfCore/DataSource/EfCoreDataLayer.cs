using System.Collections;
using System.Text.RegularExpressions;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.EfCore;
using QBCore.Extensions.Internals;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public sealed class EfCoreDataLayer : IDataLayerInfo
{
	public static IDataLayerInfo Default => Static.Instance;

	public string Name => "EntityFrameworkCore";
	public Type DataContextInterfaceType => typeof(IEfCoreDataContext);
	public Type DataContextProviderInterfaceType => typeof(IEfCoreDataContextProvider);
	
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

	private EfCoreDataLayer()
	{
		_isDocumentType = IsDocumentTypeImplementation;
		_getDefaultDBSideContainerName = type => throw new NotSupportedException(nameof(GetDefaultDBSideContainerName) + " is not supported by EF data layer.");
	}

	public DSDocumentInfo CreateDocumentInfo(Type documentType)
	{
		return new EfCoreDocumentInfo(documentType);
	}

	public IQueryBuilderFactory CreateQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		return new EfCoreQBFactory(dsTypeInfo, options, insertBuilderMethod, selectBuilderMethod, updateBuilderMethod, deleteBuilderMethod, softDelBuilderMethod, restoreBuilderMethod, lazyInitialization);
	}

	private bool IsDocumentTypeImplementation(Type type)
	{
		if (type.IsEnum || type.IsTuple() || type.IsAnonymous()) return false;
		if (ArgumentHelper.IsStandardValueType(type)) return false;
		if (ArgumentHelper.IsStandardRefType(type)) return false;
		if (type == typeof(NotSupported)) return false;
		if (StaticFactory.Internals.DocumentExclusionSelector(type)) return false;

		Type gtd;
		foreach (var i in type.GetInterfaces())
		{
			if (i == typeof(IEnumerable)) return false;
			if (i.IsGenericType)
			{
				gtd = i.GetGenericTypeDefinition();
				if (gtd == typeof(IEnumerable<>) || gtd == typeof(Nullable<>)) return false;
			}
		}

		if (type.IsDefined(typeof(DsNotDocumentAttribute), false)) return false;

		return true;
	}

	static class Static
	{
		public static readonly EfCoreDataLayer Instance = new EfCoreDataLayer();

		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Static() { }
	}
}