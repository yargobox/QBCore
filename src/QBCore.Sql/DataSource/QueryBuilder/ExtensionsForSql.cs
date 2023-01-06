namespace QBCore.DataSource.QueryBuilder;

internal static class ExtensionsForSql
{
	public static string GetDBSideName(this DEPath path)
	{
		return ((SqlDEInfo)path.Single())?.DBSideName!;
	}

	public static (string Schema, string Object) ParseDbObjectName(string dbObjectName)
	{
		var i = dbObjectName?.IndexOf('.') ?? throw new ArgumentNullException(nameof(dbObjectName));
		if (i < 0)
		{
			return (string.Empty, dbObjectName);
		}
		if (i > 0 && i + 1 < dbObjectName.Length)
		{
			return (dbObjectName.Substring(0, i), dbObjectName.Substring(i + 1));
		}
		throw new ArgumentException(nameof(dbObjectName));
	}
}