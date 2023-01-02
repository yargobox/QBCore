using System.Data;
using Npgsql;
using NpgsqlTypes;
using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace QBCore.Configuration;

/// <summary>
/// Data context interface of the PostgreSQL data layer
/// </summary>
public interface IPgSqlDataContext : IDataContext
{
}

/// <summary>
/// Data context provider interfaace of the PostgreSQL data layer
/// </summary>
/// <remarks>
/// Client code must implement this interface as a <see cref="PgSqlDataContext" /> object factory
/// and add it to a DI container with a singleton lifecycle.
/// </remarks>
public interface IPgSqlDataContextProvider : IDataContextProvider, ITransient<IPgSqlDataContextProvider>, IAsyncDisposable
{
}

/// <summary>
/// Implementation of the data context interface of the PostgreSQL data layer
/// </summary>
public class PgSqlDataContext : DataContext, IPgSqlDataContext
{
	public PgSqlDataContext(NpgsqlDataSource context, string dataContextName = "default", IReadOnlyDictionary<string, object?>? args = null)
		: base(context, dataContextName, args)
	{
	}
}

public static class ExtensionsForPgSqlDataContext
{
	/// <summary>
	/// Convert the IDataContext.Context property to <see cref="NpgsqlDataSource" />.
	/// </summary>
	/// <param name="dataContext"></param>
	/// <returns><see cref="NpgsqlDataSource" /></returns>
	/// <exception cref="ArgumentException">when dataContext or dataContext.Context is null or no conversion is possible</exception>
	public static NpgsqlDataSource AsNpgsqlDataSource(this IDataContext dataContext)
	{
		return dataContext?.Context as NpgsqlDataSource ?? throw new ArgumentException(nameof(dataContext));
	}

	public static bool IsDbTypeOneOfIntegerTypes(this QBParameter parameter)
	{
		if (parameter.DbType is DbType dbType)
		{
			switch (dbType)
			{
				case DbType.Int32:
				case DbType.Int64:
				case DbType.UInt32:
				case DbType.UInt64:
				case DbType.Byte:
				case DbType.SByte:
				case DbType.Int16:
				case DbType.UInt16: return true;
			}
		}
		else if (parameter.DbType is NpgsqlDbType npgsqlDbType)
		{
			switch (npgsqlDbType)
			{
				case NpgsqlDbType.Integer:
				case NpgsqlDbType.Bigint:
				case NpgsqlDbType.Smallint: return true;
			}
		}

		return false;
	}
}