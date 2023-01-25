using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NpgsqlTypes;
using QBCore.Configuration;
using QBCore.DataSource.QueryBuilder;
using QBCore.DataSource.QueryBuilder.PgSql;
using QBCore.Extensions.Internals;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public sealed class PgSqlDataLayer : IDataLayerInfo
{
	public static IDataLayerInfo Default => Static.Instance;

	public string Name => "PostgreSQL";
	public Type DataContextInterfaceType => typeof(IPgSqlDataContext);
	public Type DataContextProviderInterfaceType => typeof(IPgSqlDataContextProvider);
	
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

	private PgSqlDataLayer()
	{
		_isDocumentType = IsDocumentTypeImplementation;
		_getDefaultDBSideContainerName = type => throw new NotSupportedException(nameof(GetDefaultDBSideContainerName) + " is not supported by PostgreSQL data layer.");
	}

	public DSDocumentInfo CreateDocumentInfo(Type documentType)
	{
		return new SqlDocumentInfo(documentType);
	}

	public IQueryBuilderFactory CreateQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		return new PgSqlQBFactory(dsTypeInfo, options, insertBuilderMethod, selectBuilderMethod, updateBuilderMethod, deleteBuilderMethod, softDelBuilderMethod, restoreBuilderMethod, lazyInitialization);
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
				if (gtd == typeof(IEnumerable<>) || gtd == typeof(Nullable<>) || gtd == typeof(NpgsqlRange<>)) return false;
			}
		}

		if (type.IsDefined(typeof(DsNotDocumentAttribute), false)) return false;

		return true;
	}

	static class Static
	{
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Static() { }

		public static readonly PgSqlDataLayer Instance = new PgSqlDataLayer();

		public static readonly HashSet<Type> _knownTypes = new()
		{
			typeof(NotSupported),
			
			typeof(Dictionary<string, string>),
			typeof(IDictionary<string, string>),

			typeof(System.Net.IPAddress),

			typeof(NpgsqlTsQuery),
			typeof(NpgsqlTsVector),

			typeof(ArraySegment<byte>),
			typeof(ArraySegment<byte>?),
			typeof(System.Numerics.BigInteger),
			typeof(System.Numerics.BigInteger?),
			typeof(ValueTuple<System.Net.IPAddress, int>), // instead typeof(NpgsqlInet)
			typeof(ValueTuple<System.Net.IPAddress, int>?),

			typeof(NpgsqlBox),
			typeof(NpgsqlBox?),
			typeof(NpgsqlCircle),
			typeof(NpgsqlCircle?),
			typeof(NpgsqlInterval),
			typeof(NpgsqlInterval?),
			typeof(NpgsqlLine),
			typeof(NpgsqlLine?),
			typeof(NpgsqlLogSequenceNumber),
			typeof(NpgsqlLogSequenceNumber?),
			typeof(NpgsqlLSeg),
			typeof(NpgsqlLSeg?),
			typeof(NpgsqlPath),
			typeof(NpgsqlPath?),
			typeof(NpgsqlPoint),
			typeof(NpgsqlPoint?),
			typeof(NpgsqlPolygon),
			typeof(NpgsqlPolygon?),
			//typeof(NpgsqlRange<>),
			typeof(NpgsqlTid),
			typeof(NpgsqlTid?)
		};
	}
}