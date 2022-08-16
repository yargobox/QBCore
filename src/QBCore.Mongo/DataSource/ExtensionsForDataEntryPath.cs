namespace QBCore.DataSource;

internal static class ExtensionsForDataEntryPath
{
	public static string GetDBSideName(this DataEntryPath path)
	{
		return string.Join('.', path.Cast<MongoDataEntry>().Select(x => x.DBSideName));
	}
}