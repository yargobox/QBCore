namespace QBCore.DataSource;

internal static class ExtensionsForDataEntryPath
{
	public static string GetDBSideName(this DEPath path)
	{
		return string.Concat("\"", string.Join("\".\"", path.Cast<SqlDEInfo>().Select(x => x.DBSideName)), "\"");
	}
}