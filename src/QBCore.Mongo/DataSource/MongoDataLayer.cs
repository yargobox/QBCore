using System.Collections;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace QBCore.DataSource;

public sealed class MongoDataLayer : IDataLayerInfo
{
	public static readonly IDataLayerInfo Default = new MongoDataLayer();

	public string Name => "Mongo";
	public Type DatabaseContextInterface => typeof(IMongoDbContext);
	public Func<Type, bool> IsDocumentType { get; set; }

	private MongoDataLayer()
	{
		IsDocumentType = IsDocumentTypeImplementation;
	}

	public DSDocumentInfo CreateDocumentInfo(Type documentType)
	{
		return new MongoDocumentInfo(documentType);
	}

	public IQueryBuilderFactory CreateQBFactory(Type dataSourceConcrete, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		return new MongoQBFactory(dataSourceConcrete, options, insertBuilderMethod, selectBuilderMethod, updateBuilderMethod, deleteBuilderMethod, softDelBuilderMethod, restoreBuilderMethod, lazyInitialization);
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