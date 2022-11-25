namespace QBCore.DataSource;

internal static class ExtensionsForDataEntryPath
{
	public static string GetDBSideName(this DEPath path)
	{
		return string.Join('.', path.Cast<EfDEInfo>().Select(x => x.DBSideName));
	}
}