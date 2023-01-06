namespace QBCore.DataSource.QueryBuilder.EfCore;

internal static class ExtensionsForEfCore
{
	public static string GetDBSideName(this DEPath path)
	{
		return ((EfCoreDEInfo)path.Single())?.DBSideName!;
	}
}