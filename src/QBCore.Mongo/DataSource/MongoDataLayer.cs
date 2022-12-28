using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.Mongo;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public sealed class MongoDataLayer : IDataLayerInfo
{
	public static IDataLayerInfo Default => Static.Instance;

	public string Name => "Mongo";
	public Type DataContextInterfaceType => typeof(IMongoDataContext);
	public Type DataContextProviderInterfaceType => typeof(IMongoDataContextProvider);
	
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

	private MongoDataLayer()
	{
		_isDocumentType = IsDocumentTypeImplementation;
		_getDefaultDBSideContainerName = type => type.GetCustomAttribute<BsonCollectionAttribute>(true)?.Name ?? type.Name;
	}

	public DSDocumentInfo CreateDocumentInfo(Type documentType)
	{
		return new MongoDocumentInfo(documentType);
	}

	public IQueryBuilderFactory CreateQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		return new MongoQBFactory(dsTypeInfo, options, insertBuilderMethod, selectBuilderMethod, updateBuilderMethod, deleteBuilderMethod, softDelBuilderMethod, restoreBuilderMethod, lazyInitialization);
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

		if (StaticFactory.Internals.DocumentExclusionSelector(type))
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

	static class Static
	{
		public static readonly MongoDataLayer Instance = new MongoDataLayer();

		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Static() { }
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
		typeof(UIntPtr),
		typeof(ObjectId),
		typeof(Decimal128)
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
		typeof(ObjectId?),
		typeof(Decimal128?),
		typeof(BsonValue),
		typeof(BsonArray),
		typeof(BsonDocument),
		typeof(BsonBinaryData),
		typeof(BsonUndefined),
		typeof(BsonTimestamp),
		typeof(BsonSymbol),
		typeof(BsonRegularExpression),
		typeof(BsonNull),
		typeof(BsonMinKey),
		typeof(BsonMaxKey),
		typeof(BsonJavaScriptWithScope),
		typeof(BsonDateTime),
		typeof(BsonJavaScript),
		typeof(Regex)
	};
}