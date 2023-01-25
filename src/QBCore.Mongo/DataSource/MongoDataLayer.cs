using System.Collections;
using System.Reflection;
using MongoDB.Bson;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.Mongo;
using QBCore.Extensions.Internals;
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
		if (type.IsEnum || type.IsTuple() || type.IsAnonymous()) return false;
		if (ArgumentHelper.IsStandardValueType(type)) return false;
		if (ArgumentHelper.IsStandardRefType(type)) return false;
		if (Static._knownTypes.Contains(type)) return false;
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
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Static() { }

		public static readonly MongoDataLayer Instance = new MongoDataLayer();

		public static readonly HashSet<Type> _knownTypes = new HashSet<Type>()
		{
			typeof(NotSupported),

			typeof(ObjectId),
			typeof(ObjectId?),
			typeof(Decimal128),
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
			typeof(BsonJavaScript)
		};
	}
}